using Jotunn;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ValheimModToDo
{
    public class ToDoListEdit
    {
        public ToDoResources todoList;

        private GameObject scrollView; // The ScrollView containing the content panel
        private Transform contentPanel; // The scrollable content panel

        private float nameWidth = 50.0f;
        private float width = 100.0f;
        private readonly float rowHeight = 16f;
        public bool viewOnly = false;
        private float buttonWidth = 16f;

        public void SetActive(bool active)
        {
            scrollView.SetActive(active);
        }

        public void AddEditMode(GameObject parent, float width, float height, float nameWidth)
        {
            viewOnly = false;
            width += 2 * buttonWidth;
            SetUp(parent, width, height, nameWidth);
        }
        public void AddViewMode(GameObject parent, float width, float height, float nameWidth)
        {
            viewOnly = true;
            SetUp(parent, width, height, nameWidth);
        }

        public void SetUp(GameObject parent, float width, float height, float nameWidth)
        {
            Jotunn.Logger.LogDebug("ToDoListEdit.SetUp");

            if (scrollView == null)
            {
                this.width = width;
                this.nameWidth = nameWidth;

                scrollView = GUIManager.Instance.CreateScrollView(
                    parent: parent.transform,
                    showVerticalScrollbar: !viewOnly,
                    showHorizontalScrollbar: false,
                    width: width,
                    height: height,
                    handleSize: 8f,
                    handleColors: GUIManager.Instance.ValheimScrollbarHandleColorBlock,
                    handleDistanceToBorder: 50f,
                    slidingAreaBackgroundColor: new Color(0.1568628f, 0.1019608f, 0.0627451f, 1f));

                contentPanel = scrollView.transform.Find("Scroll View/Viewport/Content");
                Jotunn.Logger.LogDebug("ToDoListEdit.SetUp: Scroll view created");
            }
        }

        public void Clear()
        {
            if (contentPanel != null)
            {
                contentPanel.DetachChildren();
            }
        }

        public void AddRow(string name, string amounts = null, string modifyItemKey = null)
        {
            GameObject newRow = new GameObject("Row", typeof(RectTransform), typeof(CanvasRenderer));
            newRow.transform.SetParent(contentPanel, false);

            HorizontalLayoutGroup layoutGroup = newRow.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 1;
            layoutGroup.childAlignment = TextAnchor.MiddleLeft;
            layoutGroup.padding = new RectOffset(0, 0, 0, 1);
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = true;

            var textColor = viewOnly ? GUIManager.Instance.ValheimBeige : GUIManager.Instance.ValheimOrange;

            GUIManager.Instance.CreateText(
                text: name,
                parent: layoutGroup.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, 0f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: textColor,
                outline: true,
                outlineColor: Color.black,
                width: nameWidth,
                height: rowHeight,
                addContentSizeFitter: false);

            var amountsWidth = width - nameWidth - layoutGroup.padding.left - layoutGroup.padding.right;
            if (modifyItemKey != null)
                amountsWidth -= (3 * buttonWidth + layoutGroup.spacing + layoutGroup.padding.left + layoutGroup.padding.right);

            GUIManager.Instance.CreateText(
                text: amounts,
                parent: layoutGroup.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, 0f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: textColor,
                outline: true,
                outlineColor: Color.black,
                width: amountsWidth,
                height: rowHeight,
                addContentSizeFitter: false);

            if (modifyItemKey != null)
            {
                var addButton = GUIManager.Instance.CreateButton(
                    text: "+",
                    parent: layoutGroup.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(0, 0f),
                    width: buttonWidth,
                    height: rowHeight);

                var subButton = GUIManager.Instance.CreateButton(
                    text: "-",
                    parent: layoutGroup.transform,
                    anchorMin: new Vector2(0f, 1f),
                    anchorMax: new Vector2(0f, 1f),
                    position: new Vector2(0, 0f),
                    width: buttonWidth,
                    height: rowHeight);

                addButton.GetComponent<Button>().onClick.AddListener(() => IncrementAmount(modifyItemKey));
                subButton.GetComponent<Button>().onClick.AddListener(() => DecrementAmount(modifyItemKey));
                addButton.SetActive(true);
                subButton.SetActive(true);
            }
        }

        public void AddLabelRow(string label)
        {
            GameObject newRow = new GameObject("Row", typeof(RectTransform), typeof(CanvasRenderer));
            newRow.transform.SetParent(contentPanel, false);

            HorizontalLayoutGroup layoutGroup = newRow.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 10;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.padding = new RectOffset(0, 0, 1, 1);
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = true;

            var textColor = viewOnly ? GUIManager.Instance.ValheimBeige : GUIManager.Instance.ValheimOrange;

            GUIManager.Instance.CreateText(
                text: label,
                parent: layoutGroup.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, 0f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 16,
                color: textColor,
                outline: true,
                outlineColor: Color.black,
                width: nameWidth,
                height: rowHeight,
                addContentSizeFitter: false);
        }

        public UnityEvent onListChanged = new();

        public void IncrementAmount(string modifyItemKey)
        {
            Jotunn.Logger.LogDebug($"ToDoListEdit.IncrementAmount({modifyItemKey})");
            if (todoList != null)
            {
                todoList.AddExistingRecipe(modifyItemKey);
                onListChanged?.Invoke();
            }
        }

        public void DecrementAmount(string modifyItemKey)
        {
            Jotunn.Logger.LogDebug($"ToDoListEdit.DecrementAmount({modifyItemKey})");
            if (todoList != null)
            {
                todoList.RemoveRecipe(modifyItemKey);
                onListChanged?.Invoke();
            }
        }
    }
}