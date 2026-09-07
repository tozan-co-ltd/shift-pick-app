using shift_pick_app.Common;
using shift_pick_app.Models;
using shift_pick_app.Properties;
using shift_pick_app.Services;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace shift_pick_app.Views;

public partial class SearchShippingInstructionListPage : ContentPage
{
    int selectedCompanyID;
    bool isItemTapped = false;
    public int handyMenuID = 0;
    private string? _handyApiUrl;
    bool hasOnSearchClick = false; // �����N���b�N
    string chosedDeliveryTimeClass = "1"; // �f�t�H���g�ł́A�ւ� 1 �ł�
    private readonly LocalDbService _dbService;
    public List<M_CompanyModel> listMCompany = new List<M_CompanyModel>();
    public List<M_ProductModel> listProducts = new List<M_ProductModel>();
    public ObservableCollection<string> placeList = new ObservableCollection<string> { "TA", "E5"};

    /// <summary>
    /// �o�׎w�����X�g����
    /// </summary>
    /// <param name="menuID">menuID</param>
    /// <param name="mCompanySqlLite">��Ѓ��X�g</param>
    /// <param name="listProductItems">�d���惊�X�g</param>
    public SearchShippingInstructionListPage(int menuID, List<M_CompanyModel> mCompanySqlLite, List<M_ProductModel> listProductItems)
	{
		InitializeComponent();
        _dbService = new LocalDbService();
        listMCompany = mCompanySqlLite;
        listProducts = listProductItems;
        handyMenuID = menuID;
        // �o�׎w�����X�g�����ݒ�
        SetSearchShippingInstruction();
    }

    /// <summary>
    /// �o�׎w�����X�g�����ݒ�
    /// </summary>
    private async void SetSearchShippingInstruction()
    {
        // ���[�J��DB�̃n���f�B�ݒ���擾
        var settings = await _dbService.GetSetting();
        if (settings.Count() > 0 && settings.Last() != null)
            _handyApiUrl = settings.Last().HandyApiUrl;

        deliveryName.ItemsSource = listMCompany.Select(c => c.CompanyName).ToList();
        deliveryName.SelectedItem = listMCompany.Select(c => c.CompanyName).ToList().FirstOrDefault();

        deliveryLocation.ItemsSource = placeList;
        deliveryLocation.SelectedItem = "TA";
        datePicker.Date = Util.GetNextday(DateTime.Now);
    }

    /// <summary>
    /// �߂�
    /// </summary>
    private void OnBackButtonClicked(object sender, EventArgs e)
    {
        if (!isItemTapped)
        {
            isItemTapped = true;
            Application.Current.MainPage = new MenuPage();
            isItemTapped = false;
        }
    }

    /// <summary>
    /// �[���ꏊ��ύX����̂�����
    /// </summary>
    private void OnDeliveryLocationChanged(object sender, EventArgs e)
    {
        deliveryLocation.SelectedItem = (string)deliveryLocation.SelectedItem;
    }

    /// <summary>
    /// �[����Ж���ύX����̂�����
    /// </summary>
    private void OnDeliveryNameChanged(object sender, EventArgs e)
    {
        // ���ID���擾
        int selectedIndex = deliveryName.SelectedIndex;
        if (selectedIndex != -1)
            selectedCompanyID = listMCompany[selectedIndex].CompanyID;

        deliveryName.SelectedItem = (string)deliveryName.SelectedItem;
    }

    /// <summary>
    /// �ւ�ύX����̂�����
    /// </summary>
    private void OnDeliveryTimeClassClicked(object sender, EventArgs e)
    {
        deliveryTimeClass1.BackgroundColor = Colors.Gray;
        deliveryTimeClass2.BackgroundColor = Colors.Gray;
        deliveryTimeClass3.BackgroundColor = Colors.Gray;
        deliveryTimeClass4.BackgroundColor = Colors.Gray;

        Button selectedDeliveryTimeClass = (Button)sender;
        selectedDeliveryTimeClass.BackgroundColor = Color.FromHex("#008577");
        chosedDeliveryTimeClass = selectedDeliveryTimeClass.Text;
    }

    /// <summary>
    /// ��������
    /// </summary>
    private async void OnSearchButtonClicked(object sender, EventArgs e)
    {
        try
        {
            if (hasOnSearchClick) return;

            // �l�b�g���[�N�ڑ��`�F�b�N
            if (!NetworkCheck.CheckNetworkConnection()) { await Application.Current.MainPage.DisplayAlert("�G���[", $"E3003: {ErrorMessagesResources.E3003}", "OK"); return;}

            //���[�h��ʕ\��
            Application.Current.MainPage = new LoadingPage();
            await Task.Delay(200);

            hasOnSearchClick = true;
            // ���[�U�[�����擾
            var loginUsers = await _dbService.GetLoginAsync();
            var loginUserSqlLite = loginUsers.Last();
            DateTime selectedDate = datePicker.Date;

            var shipmentScheduleRequestBody = new
            {
                CompanyID = selectedCompanyID,
                CompanyCode = loginUserSqlLite.CompanyCode,
                CompanyPassword = loginUserSqlLite.CompanyPassword,
                DepoID = loginUserSqlLite.DepoID,
                DeliveryDate = selectedDate,
                DeliveryTimeClass = chosedDeliveryTimeClass,
                DeliveryLocation = (string)deliveryLocation.SelectedItem,
            };

            var jsonDataSend = JsonConvert.SerializeObject(shipmentScheduleRequestBody);

            // API�Ƀ��N�G�X�g�𑗂�
            var response = await App.API.PostMethod(jsonDataSend, _handyApiUrl, "ShipmentSchedule/Find");
            if (response.success)
            {
                // API����R���e���c���擾
                var contentFromApi = JsonConvert.DeserializeObject<ShipmentScheduleModel.ShipmentScheduelResponseBody>(response.content);
                var shipmentScheduleList = contentFromApi.D_ShipmentScheduleList;
                // �[�����ԍ�
                var deliverySlipNumber = shipmentScheduleList.FirstOrDefault().DeliverySlipNumber;            
                Application.Current.MainPage = new DisplayShippingInstructionListPage(handyMenuID, selectedCompanyID, (string)deliveryName.SelectedItem,selectedDate, chosedDeliveryTimeClass, (string)deliveryLocation.SelectedItem, listMCompany, shipmentScheduleList, listProducts, deliverySlipNumber);
            }
            else
            {
                await DisplayAlert("�G���[", response.content, "OK");
                Application.Current.MainPage = new SearchShippingInstructionListPage(handyMenuID, listMCompany, listProducts);
            }

            hasOnSearchClick = false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("�G���[", $"E9999: {ErrorMessagesResources.E9999} \n" + ex.Message, "OK");
            Application.Current.MainPage = new SearchShippingInstructionListPage(handyMenuID, listMCompany, listProducts);
        }
    }
}
