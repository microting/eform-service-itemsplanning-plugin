namespace ServiceItemsPlanningPlugin.Messages
{
    public class ItemCaseCreate
    {
        public int itemId { get; }
        public int itemListId { get; }

        public ItemCaseCreate(int itemListId, int itemId)
        {
            this.itemListId = itemListId;
            this.itemId = itemId;
        }
        
    }
}