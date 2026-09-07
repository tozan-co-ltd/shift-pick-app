using Android.App.Job;
using shift_pick_app.Common;
using shift_pick_app.Models;
using shift_pick_app.Properties;
using shift_pick_app.Services;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using static shift_pick_app.Models.D_StoreInModel;
using static shift_pick_app.Models.HandySettingModel;

namespace shift_pick_app.Views;

public partial class RegisterReservePage : ContentPage
{
    int countOfScaned = 0;
    //���ɔ����̒P�ʂ͕K��1
    int numOfBoxes = 1;
    bool isItemTapped = false;
    private string? _handyApiUrl;
    private bool _IsScanPageActive = false;
    public int handyMenuID = 0;

    /// <summary>
    /// �n���f�B�ݒ���
    /// </summary>
    private HandySettingModel.HandySetting _handySetting;

    /// <summary>
    /// �����W�v�\�����X�g
    /// </summary>
    private ObservableCollection<D_ReceiveModel> _TotalNumberOfBoxesDisplay = new ObservableCollection<D_ReceiveModel>();

    /// <summary>
    /// �����ꗗ�\�����X�g
    /// </summary>
    private ObservableCollection<D_ReceiveModel> _HistoryListDisplay = new ObservableCollection<D_ReceiveModel>();

    private readonly LocalDbService _dbService = new LocalDbService();
    public D_StoreInModel.StoreInPostBody storeInPostBody = new D_StoreInModel.StoreInPostBody();
    public List<M_ProductModel> listProducts = new List<M_ProductModel>();
    public List<D_HandyErrorMessageModel> listDHandyErrorMessage = new List<D_HandyErrorMessageModel>();
    public List<ScanReceiveModel> listScanReceive = new List<ScanReceiveModel>();
    public M_SupplierKanbanModel mSupplierKanban;
    public List<M_SupplierKanbanModel> listMSupplierKanban;
    public List<M_SupplierKanbanModel> listMSupplierKanbanForCheckQRCode;
    public List<R_HandyMenuSupplierKanbanModel> listRHandyMenuSupplierKanban;
    public RegisterReservePage()
	{
		InitializeComponent();
	}

    /// <summary>
    /// ��ʂ����J������C�x���g���T�u�X�N���C�u
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _IsScanPageActive = true;
    }

    /// <summary>
    /// �y�[�W���\������Ȃ��ꍇ�̓C�x���g�̍w�ǂ�����
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _IsScanPageActive = false;
    }

    /// <summary>
    /// �߂�{�^������
    /// </summary>
    private async void OnButtonBackClick(object sender, EventArgs e)
    {
        if (!isItemTapped)
        {
            isItemTapped = true;
            if (_TotalNumberOfBoxesDisplay.Count() > 0)
            {
                bool clipboardContent = await DisplayAlert("�m�F", "�ǂݎ�����f�[�^�͍폜����܂����A�߂��Ă�낵���ł����B", "OK", "�L�����Z��");
                if (clipboardContent)
                    Application.Current.MainPage = new MenuPage();
            }
            else
                Application.Current.MainPage = new MenuPage();

            isItemTapped = false;
        }
    }




    /// <summary>
    /// �ǎ���у��f���쐬
    /// </summary>
    /// <param name="d_ScanResultModel">�ǎ���у��f��</param>
    /// <param name="depoID">�q��ID</param>
    /// <param name="numberOfBoxes">���͔���</param>
    /// <param name="clipboardContent">�N���b�v�{�[�h�̓��e</param>
    /// <param name="userName">���[�U�[��</param>
    /// <param name="supplierKanbanID">�d���悩��΂�ID</param>
    /// <returns></returns>
    public D_ScanResultModel SetScanResultModel(D_ScanResultModel d_ScanResultModel, int depoID, int numberOfBoxes, string clipboardContent, string userName, int supplierKanbanID)
    {
        d_ScanResultModel.DepoID = depoID;
        d_ScanResultModel.HandyMenuID = handyMenuID;
        d_ScanResultModel.SupplierKanbanID = supplierKanbanID;
        d_ScanResultModel.NumberOfInputBoxes = numberOfBoxes;
        d_ScanResultModel.FirstScanedString = clipboardContent;
        d_ScanResultModel.ScanedAt = DateTime.Now;
        d_ScanResultModel.CreatedAt = DateTime.Now;
        d_ScanResultModel.CreatedBy = userName;

        return d_ScanResultModel;
    }


    /// <summary>
    /// ���׎��у��f���쐬
    /// </summary>
    /// <param name="d_ReceiveModel">���׎��у��f��</param>
    /// <param name="supplierProductNumber">�d����i��</param>
    /// <param name="numberOfBoxes">���͔���</param>
    /// <param name="quantity">����</param>
    /// <returns></returns>
    public D_ReceiveModel SetDReceiveModel(D_ReceiveModel d_ReceiveModel, string supplierProductNumber, int numberOfBoxes, string userName,
        int quantity, int companyID, string mainProductKey, string fisrtSubProductKey, string secondSubProductKey)
    {
        d_ReceiveModel.CompanyID = companyID;
        d_ReceiveModel.ReceiveDate = DateTime.Now;
        d_ReceiveModel.SupplierProductNumber = supplierProductNumber;
        d_ReceiveModel.NumberOfBoxes = numberOfBoxes;
        d_ReceiveModel.Quantity = quantity;
        d_ReceiveModel.MainProductKey = mainProductKey;
        d_ReceiveModel.FirstSubProductKey = fisrtSubProductKey;
        d_ReceiveModel.SecondSubProductKey = secondSubProductKey;
        d_ReceiveModel.ScanedAt = DateTime.Now;
        d_ReceiveModel.CreatedAt = DateTime.Now;
        d_ReceiveModel.CreatedBy = userName;

        return d_ReceiveModel;
    }


    /// <summary>
    /// �n���f�B���b�Z�[�W��ݒ�
    /// </summary>
    /// <param name="d_HandyErrorMessageModel">�n���f�B�G���[���b�Z�[�W���f��</param>
    /// <param name="loginUser">���O�C���̃��[�U�[</param>
    /// <param name="clipboardContent">�N���b�v�{�[�h�̓��e</param>
    /// <param name="scanTextError">�X�L�����e�L�X�g�G���[</param>
    /// <returns></returns>
    public D_HandyErrorMessageModel SetHandyErrorMessage(List<LoginModel.LoginUserSqlLite> loginUser, string clipboardContent, string scanTextError)
    {
        D_HandyErrorMessageModel d_HandyErrorMessageModel = new();
        d_HandyErrorMessageModel.DepoID = loginUser.Last().DepoID;
        d_HandyErrorMessageModel.HandyMenuID = handyMenuID;
        d_HandyErrorMessageModel.FirstScanedString = clipboardContent;
        d_HandyErrorMessageModel.ErrorMessage = scanTextError;
        d_HandyErrorMessageModel.CreatedAt = DateTime.Now;
        d_HandyErrorMessageModel.CreatedBy = loginUser.Last().UserName;

        return d_HandyErrorMessageModel;
    }


    /// <summary>
    /// ���Ƀf�[�^�o�^
    /// </summary>
    private async void OnButtonRegisterClick(object sender, EventArgs e)
    {
        try
        {
            if (_TotalNumberOfBoxesDisplay.Count() > 0)
            {
                var settings = await _dbService.GetSetting();
                storeInPostBody.CompanyCode = settings.Last().CompanyCode;
                storeInPostBody.CompanyPassword = settings.Last().CompanyPassword;
                storeInPostBody.ScanReceive = listScanReceive;
                storeInPostBody.HandyErrorMessage = listDHandyErrorMessage;
                var jsonDataSend = JsonConvert.SerializeObject(storeInPostBody);

                // �l�b�g���[�N�ڑ��`�F�b�N
                if (!NetworkCheck.CheckNetworkConnection())
                {
                    await DisplayAlert("�G���[", $"E3003: {ErrorMessagesResources.E3003}", "OK");
                    return;
                }

                // API�Ƀ��N�G�X�g�𑗂�
                var response = await App.API.PostMethod(jsonDataSend, _handyApiUrl, "StoreIn/Register");
                // API����R���e���c���擾
                if (!response.success)
                {
                    await DisplayAlert("�G���[", response.content, "OK");
                    return;
                }

                // OK�T�E���h��炷
                string resultOkSound = await GetPathSound.GetPathOkSoundAsync(_dbService, _handySetting);
                SoundPlayer.PlaySound(resultOkSound);
                await Application.Current.MainPage.DisplayAlert("����", CreateRegisteredMessage(response.content), "OK");
                Application.Current.MainPage = new LoadingPage();
                await Task.Delay(200);
                Application.Current.MainPage = new RegisterStoreInPage(handyMenuID, listProducts);
            }
            else
            {
                // ERROR�T�E���h��炷
                string resultErrorSound = await GetPathSound.GetPathErrorSoundAsync(_dbService, _handySetting);
                SoundPlayer.PlaySound(resultErrorSound);
                await Application.Current.MainPage.DisplayAlert("�G���[", $"E2016: {ErrorMessagesResources.E2016}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("�G���[", $"E9999: {ErrorMessagesResources.E9999}\n" + ex.Message, "OK");
            return;
        }
    }
    /// <summary>
    /// �f�[�^�o�^�������̃��b�Z�[�W�����X�|���X�ɂ���ĕς���
    /// </summary>
    /// <param name="responceContent"></param>
    /// <returns></returns>
    private string CreateRegisteredMessage(string responceContent)
    {
        string defaultMessage = "�f�[�^�o�^���������܂����B";
        try
        {
            // content��JSON�I�u�W�F�N�g�Ƀf�V���A���C�Y
            var storeInResponseContent = JsonConvert.DeserializeObject<D_StoreInModel.StoreInResponceContent>(responceContent);
            if (storeInResponseContent != null)
            {
                string strDuplicateProduct = string.Empty;
                if (storeInResponseContent.ScanReceive != null)
                {
                    strDuplicateProduct = "�y����o�^����Ȃ������i�ԁz \n";
                    //�d����������΂�V���A���Ɣ[����i�Ԃ𗅗�                    
                    var duplicateProduct = storeInResponseContent.ScanReceive
                    .Select(p => p.d_ReceiveModel).Select(p => new { p.SupplierProductNumber, p.MainProductKey, p.FirstSubProductKey, p.SecondSubProductKey }).ToList();
                    // ���ʂ̕\��
                    foreach (var product in duplicateProduct)
                    {
                        strDuplicateProduct += $"�d����i��: {product.SupplierProductNumber} \n"
                                            + $"���C���L�[: {product.MainProductKey}" + "\n"
                                            + $"�T�u�L�[�P: {product.FirstSubProductKey}" + "\n"
                                            + $"�T�u�L�[�Q: {product.SecondSubProductKey}" + "\n"
                                            + "-------------------------------" + "\n";
                    }
                }
                return defaultMessage + '\n' + '\n' + storeInResponseContent.Message + '\n' + '\n' + strDuplicateProduct;
            }
            return defaultMessage;
        }
        catch (JsonException)
        {
            return defaultMessage;
        }
    }

    /// <summary>
    /// �����W�v�\���{�^��������
    /// </summary>
    private void OnTotalNumberOfBoxesBtnClicked(object sender, EventArgs e)
    {
        // �{�^���̐F�E�\�����e�ύX

    }

    /// <summary>
    /// �����ꗗ�\���{�^��������
    /// </summary>
    private void OnHistoryListBtnBClicked(object sender, EventArgs e)
    {
        // �{�^���̐F�E�\�����e�ύX
        //Application.Current.MainPage = new RegisterReservePage(0, new List<M_ProductModel>());
    }
}