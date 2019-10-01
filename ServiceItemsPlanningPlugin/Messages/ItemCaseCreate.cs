namespace ServiceItemsPlanningPlugin.Messages
{
    public class ItemCaseCreate
    {
        public int ItemId { get; }
        public int ItemListId { get; }
        public int RelatedEFormId { get; set; }
        public string Name { get; set; }

        public ItemCaseCreate(int itemListId, int itemId, int relatedEFormId, string name)
        {
            ItemListId = itemListId;
            ItemId = itemId;
            RelatedEFormId = relatedEFormId;
            Name = name;
        }
        
    }
}