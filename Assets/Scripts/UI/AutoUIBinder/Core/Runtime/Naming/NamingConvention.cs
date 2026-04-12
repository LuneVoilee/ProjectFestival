namespace AutoUI
{
    /// <summary>
    /// 组件Key命名规则枚举
    /// </summary>
    public enum NamingConvention
    {
        /// <summary>
        /// 节点名_组件类型 (例: LoginButton_Button)
        /// </summary>
        NodeName_ComponentType,

        /// <summary>
        /// 组件类型_节点名 (例: Button_LoginButton)
        /// </summary>
        ComponentType_NodeName,

        /// <summary>
        /// 仅节点名 (例: LoginButton)
        /// </summary>
        NodeNameOnly,

        /// <summary>
        /// 大驼峰 (例: LoginButtonButton)
        /// </summary>
        PascalCase,

        /// <summary>
        /// m_大驼峰 (例: m_LoginButtonButton)
        /// </summary>
        m_PascalCase,

        /// <summary>
        /// 小驼峰 (例: loginButtonButton)
        /// </summary>
        CamelCase
    }
}
