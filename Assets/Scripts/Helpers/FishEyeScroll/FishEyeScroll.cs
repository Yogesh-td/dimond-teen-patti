using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
    public class FishEyeScroll : MonoBehaviour, IEndDragHandler
    {
        [Space]
        [SerializeField] ScrollRect scroll_rect;

        [Header("Adjustable values")]
        [SerializeField] int cell_initial_index;
        [SerializeField] float cell_scalar_senstivity;

        FishEyeScroll_Cell cell_nearest;
        FishEyeScroll_Cell[] all_cells;
        HorizontalLayoutGroup cells_parent_layout;

        Vector2 content_position;
        bool should_update_nearest;
        bool isInitialized = false;

        private Vector2 Get_Size() => GetComponent<RectTransform>().rect.size;
        private Vector2 Get_Content_Position() => scroll_rect.content.anchoredPosition;
        private float Get_Content_Velocity() => scroll_rect.velocity.magnitude;


        private void Start()
        {
            Initialize();
        }
        private void Initialize()
        {
            cells_parent_layout = scroll_rect.content.GetComponent<HorizontalLayoutGroup>();
            all_cells = scroll_rect.content.GetComponentsInChildren<FishEyeScroll_Cell>();

            float cell_width = all_cells[0].Get_Size().x;
            float cell_spacing = cells_parent_layout.spacing;
            float side_spacing = (Get_Size().x * 0.5f) - (all_cells[0].Get_Size().x * 0.5f);

            float total_scrollable = (cell_width * all_cells.Length) + (cell_spacing * (all_cells.Length - 1)) + (side_spacing * 2) - Get_Size().x;

            cells_parent_layout.padding.left = (int)side_spacing;
            cells_parent_layout.padding.right = (int)side_spacing;

            LayoutRebuilder.ForceRebuildLayoutImmediate(cells_parent_layout.GetComponent<RectTransform>());

            float gap_ratio = 1f / (all_cells.Length - 1);
            for (int i = 0; i < all_cells.Length; i++)
                all_cells[i].Update_Centralized_Position(-i * gap_ratio * total_scrollable);

            Refresh(false);
            isInitialized = true;
        }

        private void Update()
        {
            if (!isInitialized)
                return;

            if (Get_Content_Position() != content_position)
                Refresh(true);

            if(should_update_nearest && Get_Content_Velocity() < 100f)
            {
                should_update_nearest = false;
                Refresh_Nearest();
            }
        }
        private void Refresh(bool shouldCheckNearest)
        {
            float nearest_distance = Mathf.Infinity;

            foreach (var cell in all_cells)
            {
                float difference = Mathf.Abs(Get_Content_Position().x - cell.Get_Centralized_Position_X());
                if (difference > cell_scalar_senstivity)
                    difference = cell_scalar_senstivity;

                float scale = (cell_scalar_senstivity - difference) / cell_scalar_senstivity;
                cell.Update_Scale(scale);

                if (shouldCheckNearest && difference < nearest_distance)
                {
                    nearest_distance = difference;
                    cell_nearest = cell;
                }
            }

            if(!shouldCheckNearest)
            {
                cell_nearest = all_cells[cell_initial_index];
                Refresh_Nearest();
            }
        }
        private void Refresh_Nearest()
        {
            scroll_rect.StopMovement();
            scroll_rect.enabled = false;
            iTween.ValueTo(this.gameObject, iTween.Hash
                (
                    "from", Get_Content_Position().x,
                    "to", cell_nearest.Get_Centralized_Position_X(),
                    "time", 0.2f,
                    "onupdate", nameof(Update_Content_Value),
                    "oncomplete", nameof(Update_Content_Completed)
                ));
        }
        private void Update_Content_Value(float value)
        {
            scroll_rect.content.anchoredPosition = new Vector2(value, scroll_rect.content.anchoredPosition.y);
        }
        private void Update_Content_Completed()
        {
            scroll_rect.enabled = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            should_update_nearest = true;
        }
    }
}