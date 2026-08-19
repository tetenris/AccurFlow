namespace AccuFlow.Models.Menu
{
    public class MenuViewModel
    {
        public Guid MenuId { get; set; }
        public Guid? MenuParentId { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public List<string> Actions { get; set; } = new List<string>();
        public int Sequence { get; set; }
        public List<MenuViewModel> ChildMenus { get; set; } = new List<MenuViewModel>();
    }
}
