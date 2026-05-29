using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = Jotunn.Logger;
using UnityEngine.UI;
using Jotunn.Managers;

namespace ValheimModToDo
{
    public class ToDoLocateResources
    {
        private readonly float radius;
        private readonly float searchIntervalSeconds;
        private float secondsSinceLastSearch = 0f;
        private bool visible = false;
        private List<string> resources = new();
        private readonly List<ContainerMarker> containerMarkers = new();

        public ToDoLocateResources(float radius, float searchIntervalSeconds)
        {
            this.radius = radius;
            this.searchIntervalSeconds = searchIntervalSeconds;
        }

        public void SetVisibility(bool visibility)
        {
            Logger.LogInfo("SetVisibility");
            this.visible = visibility;
        }

        public void UpdateMarkers()
        {
            if (!this.visible)
            {
                this.ClearMarkers();
            }

            Player player = Player.m_localPlayer;
            if (player == null) { return; }
            Vector3 playerPosition = player.transform.position;

            this.secondsSinceLastSearch += Time.deltaTime;
            if (this.secondsSinceLastSearch >= this.searchIntervalSeconds)
            {
                this.UpdateContainerPositions(playerPosition);
            }
            this.UpdateMarkers(playerPosition);
        }

        private void ForceUpdateMarkers()
        {
            this.secondsSinceLastSearch = this.searchIntervalSeconds;
            this.UpdateMarkers();
        }

        public void UpdateSearchedResources(List<string> resource_names)
        {
            Logger.LogInfo("UpdateSearchedResources");
            Logger.LogInfo($"Searching for {resource_names.Count()} now");
            this.resources = resource_names;
            this.ForceUpdateMarkers();
        }

        private void UpdateContainerPositions(Vector3 playerPosition)
        {
            Logger.LogInfo("UpdateResourceLocations");

            this.secondsSinceLastSearch = 0f;
            this.ClearMarkers();

            IEnumerable<Container> containers = SearchContainersInRadius(playerPosition);
            foreach (Container container in containers)
            {
                Vector3 containerPosition = container.transform.position;
                Dictionary<string, int> contents = new();
                foreach (string resource in this.resources)
                {
                    // check if container contains any resources we look for
                    string containerResourceName = ConvertResourceName(resource);
                    int amount = container.GetInventory().CountItems(containerResourceName);
                    if (amount != 0)
                    {
                        // store resource amount
                        contents.TryGetValue(resource, out int current_amount);
                        contents[resource] = current_amount + amount;
                    }
                }
                if (contents.Count() != 0)
                {
                    // if container has any resources we need, add a marker for it
                    ContainerMarker marker = new(containerPosition, contents);
                    this.containerMarkers.Add(marker);
                }
            }

            Logger.LogInfo($"Found {this.containerMarkers.Count} resource locations");
        }

        private static string ConvertResourceName(string todoName)
        {
            // passed resource names have simple format to display (e.g. "Wood"), but containers
            // have a different format (e.g. "$item_wood"). So we have to convert the name.
            return $"$item_{todoName.ToLower()}";
        }

        private IEnumerable<Container> SearchContainersInRadius(Vector3 player_position)
        {
            List<Piece> pieces = new();
            Piece.GetAllPiecesInRadius(
                player_position,
                this.radius,
                pieces
            );
            foreach (Piece piece in pieces)
            {
                Container container = piece.GetComponent<Container>();
                if (container != null)
                {
                    yield return container;
                }
            }
        }

        private void UpdateMarkers(Vector3 playerPosition)
        {
            foreach (ContainerMarker marker in this.containerMarkers)
            {
                marker.Update(playerPosition, this.radius);
            }
        }

        private void ClearMarkers()
        {
            foreach (ContainerMarker marker in this.containerMarkers)
            {
                marker.DestroyGameObject();
            }
            this.containerMarkers.Clear();
        }
    }

    class ContainerMarker
    {
        private readonly Vector3 containerPosition;
        private readonly GameObject markerObj;

        public ContainerMarker(Vector3 containerPosition, Dictionary<string, int> containerContents)
        {
            this.containerPosition = containerPosition;
            this.markerObj = CreateMarkerObject();
            string description = CreateDescription(containerContents);
            AddMarkerText(markerObj, description);
            markerObj.SetActive(false);
        }

        public void Update(Vector3 playerPosition, float searchRadius)
        {
            // check if marker is on screen
            Vector3 screenPos = Camera.main.WorldToScreenPointScaled(this.containerPosition);
            if (!IsOnScreen(screenPos))
            {
                this.markerObj.SetActive(false);
                return;
            }
            this.markerObj.SetActive(true);

            // update marker position
            Vector2 guiPos = TransformScreenToGuiPos(screenPos);
            RectTransform markerRect = this.markerObj.GetComponent<RectTransform>();
            markerRect.anchoredPosition = guiPos;

            // update marker size
            float scale = GetScalingFactor(playerPosition, searchRadius);
            scale = Mathf.Max(scale, 0.3f); // don't make it smaller than 30%
            this.markerObj.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private static bool IsOnScreen(Vector3 screenPos)
        {
            return screenPos.z >= 0
                && screenPos.x >= 0 && screenPos.x < Screen.width
                && screenPos.y >= 0 && screenPos.y < Screen.height;
        }

        private static Vector2 TransformScreenToGuiPos(Vector2 screenPos)
        {
            RectTransform canvasRect = GUIManager.CustomGUIBack.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                null,
                out Vector2 localPos
            );
            return localPos;
        }

        private float GetScalingFactor(Vector3 playerPosition, float searchRadius)
        {
            float absoluteDistance = playerPosition.DistanceTo(this.containerPosition);
            float relativeDistance = absoluteDistance / searchRadius;
            return 1.0f - relativeDistance;
        }

        private static GameObject CreateMarkerObject()
        {
            GameObject markerObj = new("ContainerMarker");
            markerObj.transform.SetParent(GUIManager.CustomGUIBack.transform, false);
            RectTransform rect = markerObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(16f, 16f);
            Image img = markerObj.AddComponent<Image>();
            img.color = GUIManager.Instance.ValheimOrange;
            return markerObj;
        }

        private static string CreateDescription(Dictionary<string, int> containerContents)
        {
            string description = "";
            int max_amount = 0;
            foreach (KeyValuePair<string, int> entry in containerContents)
            {
                if (entry.Value > max_amount)
                {
                    description = entry.Key;
                    max_amount = entry.Value;
                }
            }
            if (containerContents.Count > 1)
            {
                description += " ...";
            }
            return description;
        }

        private static void AddMarkerText(GameObject marker, string description)
        {
            GUIManager.Instance.CreateText(
                text: description,
                parent: marker.transform,
                anchorMin: new Vector2(1f, 0.5f),
                anchorMax: new Vector2(1f, 0.5f),
                position: new Vector2(180f, -4f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 28,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: 350f,
                height: 40f,
                addContentSizeFitter: false
            );
        }

        public void DestroyGameObject()
        {
            Object.Destroy(this.markerObj);
            // text does not need to be destroyed, it's child of marker
        }
    }
}
