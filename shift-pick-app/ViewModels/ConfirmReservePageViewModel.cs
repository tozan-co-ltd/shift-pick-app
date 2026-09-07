using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using shift_pick_app.Models;

namespace shift_pick_app.ViewModels
{
    public class ConfirmReservePageViewModel : BaseViewModel
    {
        public ObservableCollection<D_OuboModel> Items { get; set; }

        public ConfirmReservePageViewModel()
        {
            Items = new ObservableCollection<D_OuboModel>
            {
                new D_OuboModel { OfficeName = "○○事業所", OuboDate = "yyyy/mm/dd", Status = "確定" },
                new D_OuboModel { OfficeName = "○○事業所", OuboDate = "yyyy/mm/dd", Status = "予約" },
                new D_OuboModel { OfficeName = "○○事業所", OuboDate = "yyyy/mm/dd", Status = "NG" },
                new D_OuboModel { OfficeName = "○○事業所", OuboDate = "yyyy/mm/dd", Status = "確定" },
            };
        }
        
    }
}
