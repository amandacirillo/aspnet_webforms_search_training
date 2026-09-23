<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SkillsSearch.aspx.cs" Inherits="SkillsSearchWeb.SkillsSearchPage" %>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <asp:Label ID="lbl_Keywords" runat="server" Font-Bold="True" Text="Enter Keywords for Search (comma-separated):"></asp:Label>
    <asp:TextBox ID="tb_Search" runat="server" AutoPostBack="True" OnTextChanged="tb_Search_TextChanged" Width="500px"></asp:TextBox>
    <asp:Button ID="btn_clear" runat="server" OnClick="btn_clear_Click" Text="Clear Selections" />
    <br /><br />

    <asp:Label ID="lbl_SelTP" runat="server" Font-Bold="True" Text="Select Testing Program"></asp:Label>
    <br />
    <asp:DropDownList ID="cb_TPs" runat="server" AutoPostBack="True" OnSelectedIndexChanged="cb_TPs_SelectedIndexChanged">
        <asp:ListItem Text="None" Value="None" />
    </asp:DropDownList>
    <br /><br />

    <asp:RadioButton ID="radioBtn_Master" runat="server" GroupName="SkillLevel" AutoPostBack="True" OnCheckedChanged="SkillLevel_CheckedChanged" Text="Mastered Only" />
    <br />
    <asp:RadioButton ID="radioBtn_Exp_or_Master" runat="server" GroupName="SkillLevel" AutoPostBack="True" OnCheckedChanged="SkillLevel_CheckedChanged" Text="Experienced or Mastered" />
    <br />
    <asp:RadioButton ID="radioBtn_Any" runat="server" GroupName="SkillLevel" Checked="True" AutoPostBack="True" OnCheckedChanged="SkillLevel_CheckedChanged" Text="Any Skill Level" />
    <br /><br />

    <asp:Label ID="lbl_SkillList" runat="server" Font-Bold="True" Text="Programming Tools"></asp:Label>
    <asp:CheckBoxList ID="ckList_ProgTools" runat="server" AutoPostBack="True" OnSelectedIndexChanged="SelectedSkills_Changed" RepeatColumns="8" />

    <asp:Label ID="lbl_Internal" runat="server" Font-Bold="True" Text="Internally Developed Software"></asp:Label>
    <asp:CheckBoxList ID="ckList_Internal" runat="server" AutoPostBack="True" OnSelectedIndexChanged="SelectedSkills_Changed" RepeatColumns="8" />

    <asp:Label ID="lbl_ExSof" runat="server" Font-Bold="True" Text="External Software"></asp:Label>
    <asp:CheckBoxList ID="ckList_Extsoft" runat="server" AutoPostBack="True" OnSelectedIndexChanged="SelectedSkills_Changed" RepeatColumns="8" />
    <br />

    <asp:Label ID="lbl_PsychSkill" runat="server" Font-Bold="True" Text="Filter Psychometric Skill List:"></asp:Label>
    <asp:TextBox ID="tb_PsychSearch" runat="server" AutoPostBack="True" OnTextChanged="tb_PsychSearch_TextChanged"></asp:TextBox>
    <br />
    <asp:CheckBoxList ID="ckList_Psych" runat="server" AutoPostBack="True" OnSelectedIndexChanged="SelectedSkills_Changed" RepeatColumns="6" />
    <br />

    <asp:Label ID="lbl_Error" runat="server" ForeColor="Red" Visible="False" />

    <asp:Label ID="lbl_Staff" runat="server" Text="Staff" Font-Bold="True" />
    <asp:GridView ID="dgv_Staff" runat="server" AutoGenerateColumns="True" />

    <br />
    <asp:Label ID="lbl_TP" runat="server" Text="Testing Programs" Font-Bold="True" />
    <asp:GridView ID="dgv_TP" runat="server" AutoGenerateColumns="True" />

</asp:Content>
