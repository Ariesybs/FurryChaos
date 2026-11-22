using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>单局规则的静态配置，运行时只读。</summary>
[CreateAssetMenu(menuName = "GameData/Match Config")]
public class MatchConfig : ScriptableObject
{
    [SerializeField, LabelText("每名被捕玩家的赎金"), Min(0)]
    private int m_RansomPerPlayer = 500;
    public int RansomPerPlayer => m_RansomPerPlayer;
}