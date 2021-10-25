<%@ Page Title="" Language="C#" MasterPageFile="~/Main.master"  AutoEventWireup="true" CodeBehind="PdfToExcelExtract.aspx.cs" Inherits="FeeScheduleManager.UI.PdfToExcelExtract" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
<%@ Register Assembly="FeeScheduleManager" Namespace="FeeScheduleManager.Controls" TagPrefix="agp" %>
<%@ Register Src="~/Controls/AgpJModal.ascx" TagPrefix="uc" TagName="JModal" %>
<%@ Register Assembly="DevExpress.Web.v17.2, Version=17.2.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>



<asp:Content ID="Content" ContentPlaceHolderID="MainContent" runat="server" >

    <asp:ScriptManager ID="ScriptManager1" runat="server" AsyncPostBackTimeout ="360000">
    </asp:ScriptManager>
    <script>

        function fnCallback() {

            document.getElementById("<%=btnSubmitFiles.ClientID %>").disabled = '';
        }
        function OnFileUploadComplete(s, e) {
            var ext = e.callbackData.split('\\').pop().split('/').pop();
            txtLinkDescription.SetText(ext);
        }

    </script>
    <asp:UpdatePanel ID="upd" runat="server" OnUnload="UpdatePanel_Unload">
        <ContentTemplate>
            <fieldset style="margin: 5% auto 0 auto; width: 1008px;">
                <legend class="Legend" style="font-size: 12px; color: #FF9933; font-weight: bold;">Fee Schedule Data PDF File</legend>
                <table style="margin: 10px auto 0px auto; height: 103px; width: 1020px">
                    <tr>
                        <td style="width: 126px; height: 5px"></td>
                        <td class="dxeTextBoxDefaultWidthSys" style="width: 160px; height: 5px">Fee Schedule PDF File&nbsp; *</td>
                        <td>
                            <dx:ASPxTextBox ID="txtLinkDescription" ClientInstanceName="txtLinkDescription" CssClass="txtlink" runat="server" Width="480px">
                            </dx:ASPxTextBox>
                        </td>
                        <td>
                            <dx:ASPxUploadControl runat="server" ID="uplFeeSchedulePdfFiles" ClientInstanceName="uplFeeSchedulePdfFiles" NullText=" File" onChange="fnCallback();"
                                AutoStartUpload="true" ShowTextBox="False" BrowseButton-Text="Upload File" ValidationSettings-AllowedFileExtensions=".pdf,.PDF"
                                OnFileUploadComplete="uplFeeSchedulePdfFiles_FileUploadComplete" Width="150px">
                                <BrowseButton Text="Upload File">
                                </BrowseButton>
                                <ClientSideEvents FileUploadComplete="OnFileUploadComplete" />
                            </dx:ASPxUploadControl>

                        </td>
                    </tr>
                </table>
            </fieldset>
            <p />
            <asp:Label ID="lblMessage" runat="server" Font-Bold="False" Font-Size="8pt" ForeColor="#FF3300" style="width: 151px;  position: relative; left: 430px" ></asp:Label>

            <div style="width: 151px; float: right; margin: 1% 0 0 1%; position: relative; right: 130px">
                <asp:Button ID="btnSubmitFiles" runat="server" Enabled="false" Text="PdfToExcel" Width="155px" ValidationGroup="g1" CausesValidation="true" OnClick="btnSubmit_Click" CssClass="button" Style="float: right" TabIndex="3" />
            </div>
            <div style="width: 151px;  position: relative; left: 430px">
                <asp:UpdateProgress ID="UpdWaitImage" runat="server" DynamicLayout="true" AssociatedUpdatePanelID="upd">
                    <ProgressTemplate>
                        <asp:Image ID="imgProgress" ImageUrl="~/Content/Images/progress.gif" runat="server" CssClass="imagealign" />
                        Please Wait for an Email for the Converted Excel File...
                    </ProgressTemplate>
                </asp:UpdateProgress>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
