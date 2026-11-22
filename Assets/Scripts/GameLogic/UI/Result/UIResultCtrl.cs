using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIResultCtrl : UIPanel
{
    public override UIDisplayMode DisplayMode => UIDisplayMode.FullScreen;

    [SerializeField] private TextMeshProUGUI m_Title;
    [SerializeField] private TextMeshProUGUI m_Summary;
    [SerializeField] private TextMeshProUGUI m_PlayerList;
    [SerializeField] private Button m_ReturnButton;

    private void Start()
    {
        m_ReturnButton.onClick.AddListener(OnClickReturn);
    }

    protected override void OnOpen(object args)
    {
        base.OnOpen(args);
        if(args is not MatchResult result)return;
        Refresh(result);
    }

    private void Refresh(MatchResult result)
    {
        if (result == null)
        {
            return;
        }

        m_Title.text = result.Reason switch
        {
            MatchEndReason.Departed => "撤离成功",
            MatchEndReason.AllCaptured => "全员被捕",
            _ => "行动结束",
        };

        m_Summary.text = $"车上货值 {result.CargoValue}\n合约奖励 {result.ContractReward}\n赎金 -{result.RansomTotal}\n可分配 {result.NetValue}";

        var sb = new StringBuilder();
        foreach (var player in result.Players)
        {
            var status = player.Status switch
            {
                PlayerMatchStatus.Aboard => "已上车",
                PlayerMatchStatus.Captured => "被关押",
                PlayerMatchStatus.Left => "中途离开",
                _ => "未上车",
            };
            sb.AppendLine($"{player.PlayerName}  {status}  +{player.Share}");
        }

        m_PlayerList.text = sb.ToString();
    }

    private void OnClickReturn()
    {
        // 房主 Stop 会断开所有客户端，各端各自回主菜单重新建房
        GameRoot.Instance.GameNet.Stop();
        GameRoot.Instance.GameFlow.ChangeState(new MainMenuState(GameRoot.Instance.GameFlow));
    }
}