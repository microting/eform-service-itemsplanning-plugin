namespace ServiceItemsGroupPlanningPlugin.Messages
{
    public class eFormRetrieved
    {
        public int caseId { get; protected set; }

        public eFormRetrieved(int caseId)
        {
            this.caseId = caseId;
        }
    }
}