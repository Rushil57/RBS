public class DispatchAgentRequest
{
    public int AgentBeingDispatched { get; set; }
    public int SubmittingAgent { get; set; }
    public string EquipmentOrder { get; set; }
    public string FormattedDispatchBody { get; set; }
    public string PageRequestingDispatch { get; set; }
    public Lead LeadToDispatch { get; set; }
    public Account AccountToDispatch { get; set; }
    //public Account AccountToDispatch { get; set; }
}
