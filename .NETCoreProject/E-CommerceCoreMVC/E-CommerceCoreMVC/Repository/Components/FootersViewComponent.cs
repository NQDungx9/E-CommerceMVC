using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceCoreMVC.Repository.Components
{
    public class FootersViewComponent : ViewComponent
    {
        public readonly DataContext _dataContext;
        public FootersViewComponent(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IViewComponentResult> InvokeAsync() => View(await _dataContext.Contacts.Where(b => b.Status == 1).FirstOrDefaultAsync());

    }
}
