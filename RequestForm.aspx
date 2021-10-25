<%@ Page Title="" Language="C#" AutoEventWireup="true" MasterPageFile="../Main.master" CodeBehind="RequestForm.aspx.cs" Inherits="BenefitSummary.UI.RequestForm" %>

<%@ Register Assembly="BenefitSummary" Namespace="BenefitSummary.Controls" TagPrefix="cc1" %>
<%@ Register Src="~/Controls/AgpJModal.ascx" TagPrefix="uc" TagName="JModal" %>
<asp:Content ID="Content" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>
    <cc1:AjaxProgress ID="prog" runat="server" EnableViewState="False"></cc1:AjaxProgress>
    <script type="text/javascript">
        function ShowProgress(what) {
                <%= prog.ClientID %>.Show(what);
        }

        function HideProgress() {
                <%= prog.ClientID %>.Hide();
        }
        function OnAfterResizing() {
            grd_MigrationRequest.AdjustControl();
        }
        function TrackingNumOnclick(TrackingNumber, RequestID) {
            pmc.PerformCallback(RequestID);
            pmc.Show();
            pmc.SetHeaderText("Migration Request Details- " + TrackingNumber);
        }

        var timeout = window.setTimeout('ASPxGridView1.PerformCallback()', 60000);
        function scheduleGridRefresh(grdViewRequestsInstance) {
            window.clearTimeout(timeout);
            timeout = window.setTimeout('ASPxGridView1.PerformCallback()', 60000);
        }
        function pageLoad() {
            enablejQueryScripts();
        }

        function init(s, e) {
            s.GetHeaderFilterPopup().Shown.AddHandler(function (s, e) { changeSize(s, e); })
        }
        function endCallback(s, e) {
            s.GetHeaderFilterPopup().Shown.AddHandler(function (s, e) { changeSize(s, e); })
        }
        function changeSize(s, e) {
            if (ASPxGridView1.cpCol == "CreatedDate" || ASPxGridView1.cpCol == "CompletedDate")
                s.SetSize(200, 250);
        }
        function disableSubmit() {
            if (Page_IsValid) {
//                var ctl = '<%= btnSubmit.ClientID %>';
                //                setTimeout(function() {
                //                    document.getElementById(ctl).disabled = true;  // enable server side  
                //                }, 50);

            } else {
                alert('Field validation issues were found. Please correct and submit again!');
            }
        }


        function OnClick(s, e, TrackingNumber, TemplateType, requestID) {
            ErrorCountStatus.PerformCallback(requestID);
            ErrorCountStatus.Show();
            ErrorCountStatus.SetHeaderText("ErrorDetails-" + TemplateType + "-" + TrackingNumber);

        }
      

        function enablejQueryScripts() {
            $(document).ready(function () {
                $('#spanMasterProgress').hide(); // hide master page loading icon
                $('#dlgRequestHelp').dialog({ autoOpen: false, width: 850, height: 500, title: 'Help' });
                //var emailElem = getOffset( document.getElementById('ctl00_ContentBody_txtEmailAddress')).top;
                //if (emailElem > 400) {
                //    $("html").css("height",""); // IE 7 fix or IE 8 running in IE 7 standards mode
                //}
            });
        }


        function checkFileSize(obj) {
            var maxSize = '<asp:Literal ID="litFnMaxSize" runat="server" Text="" />';
            var path = obj.value;
            var fn = path.replace(/^.*[\\\/]/, '');
            var fnLength = fn.length;
            if (fnLength && fnLength > maxSize) {
                alert('The supplied filename exceeds the maximum allowed size of ' + maxSize + ' characters.  Please rename and try again.');
                var ctrlId = '<%=uplFeeScheduleFile.ClientID%>';
                if ($.browser.msie) {
                    document.getElementById(ctrlId).createTextRange().execCommand('delete');
                } else {
                    $("#" + ctrlId).val("");
                }
            }
        }

        function ddlTemplateTypes_onchange(e) {
            var ddlTemplates = document.getElementById('<%= ddlTemplateTypes.ClientID %>');
            var hiddenTemplateID = document.getElementById('<%= hdnTemplateID.ClientID %>');
            var selectedValue = ddlTemplates.value;
            hiddenTemplateID.value = selectedValue;
            document.getElementById('<%= hdnTemplateID.ClientID %>').value = selectedValue;
            __doPostBack("<%= ddlTemplateTypes.UniqueID %>", "");
        }

        $(function () {
            $('#aspnetForm').submit(function () {
                disableSubmit();
            });

            //var cookieFound = ($.cookie('HpssBsbsHelpCookie')) ? true : false;
            //if (!cookieFound) {
            //    $.cookie('HpssBsbsHelpCookie', 'set', { expires: 9999 });
            //    $('#dlgVideoIntro').dialog({ autoOpen: false, width: 500, height: 380, title: 'Introduction Video (3 minutes)', close: function(ev, ui) { $(this).dialog('destroy'); } });
            //    $('#dlgVideoIntro').dialog("open");
            //}
        });


        function copyToClipBoard(text) {
            text = text.replace(/\*/g, "\\");
            window.prompt("Please copy (Ctrl+C for Windows or Cmd+C for Mac) reports location to clipboard then paste (Ctrl+V or Cmd+V) into Windows Explorer or your browser to see associated reports...", text);
        }

    </script>
    <div id="dlgRequestHelp" title="Help" style="display: none;">
        <div id="pRequestHelp" style="clear: right; font-family: Calibri; font-size: 12px;"></div>
    </div>
    <asp:UpdatePanel ID="upd" runat="server" OnUnload="UpdatePanel_Unload">
        <Triggers>
            <asp:PostBackTrigger ControlID="btnSubmit" />
        </Triggers>
        <ContentTemplate>
            <div style="margin-left: auto; margin-right: auto; width: 1149px;">
                <span id="title">
                    <h3 style="color: #0066FF">Submit Request <span>&nbsp;&nbsp;&nbsp;</span></h3>
                </span>

                <fieldset style="margin-left: auto; margin-right: auto; width: 1143px;">
                    <legend class="Legend" style="font-size: 12px; color: #FF9933; font-weight: bold;">Request Information</legend>
                    <table style="margin-left: auto; margin-right: auto; height: 53px; width: 1149px">
                        <tr>
                            <td style="height: 4px; width: 26px;"></td>
                            <td class="dxeTextBoxDefaultWidthSys" style="height: 4px; width: 155px">Template Type *</td>
                            <td style="height: 4px; width: 368px">
                                <br />
                                <asp:DropDownList ID="ddlTemplateTypes" runat="server"
                                    onchange="ddlTemplateTypes_onchange()"
                                    OnSelectedIndexChanged="ddlTemplateTypes_SelectedIndexChanged" />
                                <asp:RequiredFieldValidator InitialValue="-1" ID="rqdTemplateTypes"
                                    ValidationGroup="g1" runat="server" ControlToValidate="ddlTemplateTypes"
                                    Text="*Required" ErrorMessage="Please specify the associated template types." ForeColor="Red" Font-Bold="True"></asp:RequiredFieldValidator>
                            </td>
                            <td style="height: 4px; width: 49px;"></td>
                            <td style="height: 4px; width: 164px">
                                <asp:HiddenField ID="hdnTemplateID" runat="server" />
                            </td>
                            <td style="height: 4px"></td>
                        </tr>
                        <tr>
                            <td style="height: 11px; width: 26px;"></td>
                            <td class="dxeTextBoxDefaultWidthSys" style="width: 155px; height: 11px;">Tracking Number *</td>
                            <td style="width: 368px; height: 11px;">
                                <br />
                                <asp:TextBox ID="txtTrackingNumber" MaxLength="20" placeholder="Max 20 characters" runat="server" Style="margin-left: 0px" Width="190px"></asp:TextBox><br />
                                <asp:RequiredFieldValidator ID="rqdTrackingNumber" Display="Dynamic" runat="server" ValidationGroup="g1"
                                    ErrorMessage=" * Required" ControlToValidate="txtTrackingNumber" Font-Bold="True" ForeColor="Red" />
                            </td>
                            <td style="height: 11px; width: 49px;"></td>
                            <td style="width: 164px; height: 11px;">
                                <asp:Label ID="lblEmailAddress" runat="server" Text="Email Receipients*"></asp:Label>
                            </td>
                            <td style="height: 11px">
                                <br />
                                <dx:ASPxTokenBox ID="tknEmailAddress" runat="server" ItemValueType="System.String" Width="232px">
                                    <ValidationSettings ErrorDisplayMode="Text" ErrorText="Please enter the valid Email ID" ErrorTextPosition="Bottom">
                                        <ErrorFrameStyle Font-Bold="True">
                                        </ErrorFrameStyle>
                                        <RegularExpression ErrorText="Please enter the valid Email ID" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" />
                                        <RequiredField IsRequired="True" ErrorText="*Please enter the Email ID" />
                                    </ValidationSettings>
                                </dx:ASPxTokenBox>
                            </td>
                        </tr>
                    </table>
                </fieldset>
                <fieldset style="margin-left: auto; margin-right: auto; width: 1143px;">
                    <legend class="Legend" style="font-size: 12px; color: #FF9933; font-weight: bold;">Benefit Summary Data File</legend>
                    <table style="margin-left: auto; margin-right: auto; height: 53px; width: 1149px">
                        <tr>
                            <td style="width: 59px; height: 5px"></td>
                            <td class="dxeTextBoxDefaultWidthSys" style="width: 227px; height: 5px">Data File *</td>
                            <td style="width: 721px; height: 5px">
                                <asp:FileUpload ID="uplFeeScheduleFile" runat="server" Height="23px" onblur="checkFileSize(this);" Style="margin-left: 0px" Width="605px" /><br />
                                <asp:RegularExpressionValidator ID="regexValidator" runat="server"
                                    ControlToValidate="uplFeeScheduleFile"
                                    ErrorMessage="* File must be xlsx file."
                                    ValidationExpression="(.*\.([Xx][Ll][Ss][Xx])$)" Font-Bold="True" ForeColor="Red"></asp:RegularExpressionValidator>
                            </td>
                            <td style="width: 721px; height: 5px">
                                <asp:HyperLink ID="hlTemplate" runat="server" Font-Size="Smaller" NavigateUrl="~/Model/PremiumRatesInputTemplate.xlsx">BS_Medicaid_Template</asp:HyperLink>
                                <asp:Image ID="Image2" runat="server" ImageUrl="~/Content/Images/22.png" />
                                <asp:HiddenField ID="HiddenField1" runat="server" />
                            </td>
                        </tr>
                    </table>
                </fieldset>

                <asp:Label ID="lblMessage" runat="server" Font-Bold="False" Font-Size="8pt" ForeColor="#FF3300"></asp:Label>
                <div>
                    <div style="text-align: left; width: 50%; clear: both; float: left; margin: 1% 0 0 0.5%">
                    </div>

                    <div style="width: 151px; float: right; margin-top: 1%; margin-left: 13px;">
                        <dx:ASPxButton ID="btnCancel" runat="server" Text="Clear" Width="55px" Style="float: left" OnClick="btnCancel_Click" CssClass="button" CausesValidation="false" />
                        &nbsp;&nbsp;&nbsp;
                    <dx:ASPxButton ID="btnSubmit" runat="server" Text="Submit " Width="55px" ValidationGroup="g1"
                        CausesValidation="true" OnClick="btnSubmit_Click" CssClass="button" Style="float: right" TabIndex="3" />

                    </div>
                </div>
            </div>

            <div>
                <table style="width: 75%; height: 50px">
                    <tr>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                </table>
            </div>
            <dx:ASPxGridView ID="grdViewRequests" runat="server" OnCustomCallback="grdViewRequests_CustomCallback" AutoGenerateColumns="False" DataSourceID="SqlDataSource1" emptydatatext="No premium rate load requests have been submitted or the current search criteria isn't applicable to any requests." ClientInstanceName="ASPxGridView1" Style="margin-left: auto; margin-right: auto;" Width="929px" OnBeforeHeaderFilterFillItems="grdViewRequests_BeforeHeaderFilterFillItems">
                <ClientSideEvents BeginCallback="scheduleGridRefresh" Init="init" EndCallback="endCallback" />
                <SettingsPager EllipsisMode="OutsideNumeric" EnableAdaptivity="True">
                    <PageSizeItemSettings Visible="True">
                    </PageSizeItemSettings>
                </SettingsPager>
                <Settings ShowFilterRow="True" />
                <Toolbars>
                    <dx:GridViewToolbar EnableAdaptivity="true" Position="Top">
                        <Items>
                            <dx:GridViewToolbarItem Command="ClearFilter" Name="Clear" />
                        </Items>
                    </dx:GridViewToolbar>
                </Toolbars>
                <%-- DXCOMMENT: Configure ASPxGridView's columns in accordance with datasource fields --%>
                <EditFormLayoutProperties AlignItemCaptionsInAllGroups="True">
                    <SettingsAdaptivity AdaptivityMode="SingleColumnWindowLimit" />
                </EditFormLayoutProperties>
                <Columns>
                    <dx:GridViewDataTextColumn FieldName="TrackingNumber" VisibleIndex="0" Width="75px" Caption="Tracking #">
                        <Settings AutoFilterCondition="Contains" />
                        <HeaderStyle Font-Bold="True" Font-Size="10pt" HorizontalAlign="Center" VerticalAlign="Middle" />
                        <CellStyle Font-Size="8pt" HorizontalAlign="Center">
                        </CellStyle>
                    </dx:GridViewDataTextColumn>
                    <dx:GridViewDataTextColumn FieldName="OriginalFileName" VisibleIndex="1" Caption="File Name">
                        <Settings AutoFilterCondition="Contains" />
                        <HeaderStyle Font-Bold="True" Font-Size="10pt" HorizontalAlign="Center" VerticalAlign="Middle" />
                        <CellStyle Font-Size="8pt" HorizontalAlign="Center">
                        </CellStyle>
                    </dx:GridViewDataTextColumn>
                    <dx:GridViewDataDateColumn FieldName="CreatedDate" VisibleIndex="4" Width="60px" Caption="Submitted Date" Settings-AllowSort="True" Settings-AllowAutoFilter="False" Settings-AllowHeaderFilter="True" SettingsHeaderFilter-Mode="DateRangePicker">
                        <PropertiesDateEdit DisplayFormatString="M/d/yyyy HH:mm:ss"></PropertiesDateEdit>
                        <Settings AllowAutoFilter="False" AllowHeaderFilter="True" AllowSort="True" />
                        <SettingsHeaderFilter>
                            <DateRangePeriodsSettings ShowFuturePeriods="false" />
                        </SettingsHeaderFilter>
                        <HeaderStyle Font-Bold="True" Font-Size="10pt" HorizontalAlign="Center" />
                        <CellStyle Font-Size="8pt" HorizontalAlign="Center">
                        </CellStyle>
                    </dx:GridViewDataDateColumn>
                    <dx:GridViewDataDateColumn FieldName="CompletedDate" VisibleIndex="5" Width="60px" Settings-AllowSort="True" Settings-AllowAutoFilter="False" Settings-AllowHeaderFilter="True" SettingsHeaderFilter-Mode="DateRangePicker">
                        <PropertiesDateEdit DisplayFormatString="M/d/yyyy HH:mm:ss"></PropertiesDateEdit>
                        <Settings AllowAutoFilter="False" AllowHeaderFilter="True" AllowSort="True" />
                        <SettingsHeaderFilter>
                            <DateRangePeriodsSettings ShowFuturePeriods="false" />
                        </SettingsHeaderFilter>
                        <HeaderStyle Font-Bold="True" Font-Size="10pt" HorizontalAlign="Center" />
                        <CellStyle Font-Size="8pt" HorizontalAlign="Center">
                        </CellStyle>
                    </dx:GridViewDataDateColumn>
                    <dx:GridViewDataTextColumn FieldName="TotalRows" VisibleIndex="6" Width="50px">
                        <Settings AutoFilterCondition="Contains" AllowAutoFilter="False" />
                        <HeaderStyle Font-Bold="True" Font-Size="10pt" HorizontalAlign="Center" />
                        <CellStyle Font-Size="8pt" HorizontalAlign="Center">
                        </CellStyle>
                    </dx:GridViewDataTextColumn>
                    <dx:GridViewDataTextColumn FieldName="ErrorCount" VisibleIndex="7" Width="50px">
                        <Settings AutoFilterCondition="Contains" AllowAutoFilter="False" />
                        <HeaderStyle Font-Bold="True" Font-Size="10pt" HorizontalAlign="Center" />
                        <CellStyle Font-Size="8pt" HorizontalAlign="Center">
                        </CellStyle>
                        <DataItemTemplate>
                            <dx:ASPxHyperLink ID="errorCount" runat="server" OnInit="errorCount_Init">
                            </dx:ASPxHyperLink>
                        </DataItemTemplate>
                    </dx:GridViewDataTextColumn>

                    <dx:GridViewDataTextColumn FieldName="WarningCount" VisibleIndex="8" Width="50px">
                        <Settings AutoFilterCondition="Contains" AllowAutoFilter="False" />
                        <HeaderStyle Font-Bold="True" Font-Size="10pt" HorizontalAlign="Center" />
                        <CellStyle Font-Size="8pt" HorizontalAlign="Center">
                        </CellStyle>
                    </dx:GridViewDataTextColumn>
                    <dx:GridViewDataHyperLinkColumn Caption="Contact" FieldName="EmailAddress" VisibleIndex="48">
                        <PropertiesHyperLinkEdit NavigateUrlFormatString="mailto:{0}">
                        </PropertiesHyperLinkEdit>
                        <Settings AutoFilterCondition="Contains" />
                        <HeaderStyle Font-Bold="True" Font-Size="10pt" HorizontalAlign="Center" />
                        <CellStyle Font-Size="8pt" HorizontalAlign="Center">
                        </CellStyle>
                    </dx:GridViewDataHyperLinkColumn>
                    <dx:GridViewDataHyperLinkColumn Caption="Reports" FieldName="ReportsPath" VisibleIndex="47" Width="20px">
                        <PropertiesHyperLinkEdit ImageUrl="~/Content/Images/icon_folder_open.png">
                        </PropertiesHyperLinkEdit>
                        <Settings AllowAutoFilter="False" />
                        <HeaderStyle Font-Bold="True" Font-Size="10pt" HorizontalAlign="Center" />
                        <CellStyle Font-Size="8pt" HorizontalAlign="Center">
                        </CellStyle>
                        <DataItemTemplate>
                            <%# this.FormatReportLink(Eval("ReportsPath").ToString())%>
                        </DataItemTemplate>
                    </dx:GridViewDataHyperLinkColumn>
                    <dx:GridViewDataComboBoxColumn FieldName="Status" VisibleIndex="3" Width="50px">
                        <PropertiesComboBox DataSourceID="SqlDataSource2" TextField="Description">
                            <DropDownButton Enabled="False"></DropDownButton>
                        </PropertiesComboBox>
                        <SettingsHeaderFilter Mode="CheckedList">
                        </SettingsHeaderFilter>
                        <Settings AllowSort="True" AllowAutoFilter="False" AllowHeaderFilter="True" />
                        <HeaderStyle Font-Bold="True" Font-Size="10pt" HorizontalAlign="Center" VerticalAlign="Middle" />
                        <CellStyle Font-Size="8pt" HorizontalAlign="Center">
                        </CellStyle>
                    </dx:GridViewDataComboBoxColumn>
                    <dx:GridViewDataComboBoxColumn FieldName="TemplateType" VisibleIndex="2">
                        <SettingsHeaderFilter Mode="CheckedList">
                        </SettingsHeaderFilter>
                        <HeaderStyle Font-Bold="True" />
                        <CellStyle Font-Size="8pt" HorizontalAlign="Center" Font-Bold="False">
                        </CellStyle>
                        <Settings AllowSort="True" AllowAutoFilter="False" AllowHeaderFilter="True" />
                    </dx:GridViewDataComboBoxColumn>
                    <dx:GridViewDataTextColumn FieldName="RequestID" VisibleIndex="21" Caption="RequestID" Visible="false">
                        <Settings AutoFilterCondition="Contains" />
                        <HeaderStyle Font-Bold="True" HorizontalAlign="Center" />
                        <CellStyle HorizontalAlign="Center">
                        </CellStyle>
                    </dx:GridViewDataTextColumn>
                    <dx:GridViewDataTextColumn VisibleIndex="49" FieldName="MIG_REQ_CNT" Caption="Migration Request">
                        <DataItemTemplate>
                            <dx:ASPxHyperLink ID="CMRCount" Text="Create Migration Request" runat="server" OnInit="CMRCount_Init">
                                <ClientSideEvents Click="on" />
                            </dx:ASPxHyperLink>
                        </DataItemTemplate>
                        <Settings AutoFilterCondition="Contains" />
                        <HeaderStyle Font-Bold="True" HorizontalAlign="Center" />
                        <CellStyle HorizontalAlign="Center">
                        </CellStyle>
                    </dx:GridViewDataTextColumn>
                </Columns>
                <Styles>
                    <AlternatingRow BackColor="#CCFFFF">
                    </AlternatingRow>
                </Styles>
                <Images>
                    <HeaderFilter Url="../Content/Images/InactiveFilter.png" Width="10px" Height="10px">
                    </HeaderFilter>
                    <HeaderActiveFilter Url="../Content/Images/ActiveFilter.png" Width="10px" Height="10px">
                    </HeaderActiveFilter>
                </Images>
            </dx:ASPxGridView>
            <asp:SqlDataSource ID="SqlDataSource2" runat="server" ConnectionString="<%$ ConnectionStrings:BsSqlConnectionString %>" SelectCommand="SELECT distinct [Description] FROM [FS_MCD].[BSBS].[RequestStatusLookup]"></asp:SqlDataSource>
            <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:BsSqlConnectionString %>" ProviderName="System.Data.SqlClient" SelectCommand="[BSBS].[selGridViewMain]" SelectCommandType="StoredProcedure"></asp:SqlDataSource>
            <dx:ASPxPopupControl ID="Popup_Mig_Creation" Style="overflow: auto;" runat="server" HeaderStyle-Font-Bold="true" ClientInstanceName="pmc" MaxWidth="1000px" MinWidth="1000px" HeaderText="Migration Request" Target="_blank" PopupAction="None" AllowResize="true" PopupHorizontalAlign="WindowCenter" CloseAnimationType="Auto" PopupVerticalAlign="WindowCenter" CloseAction="OuterMouseClick" OnWindowCallback="Popup_Mig_Crea_WindowCallback" Theme="Default" CloseOnEscape="True">
                <ClientSideEvents AfterResizing="OnAfterResizing" />
                <HeaderStyle Font-Bold="True"></HeaderStyle>
                <ContentCollection>
                    <dx:PopupControlContentControl runat="server">
                        <div id="dvTable1" runat="server">
                            <fieldset style="margin-left: auto; margin-right: auto;">
                                <legend class="Legend" style="font-size: 12px; color: #FF9933; font-weight: bold;">Create Run Request</legend>
                                <div>
                                    <table style="left: auto; right: auto;">
                                        <tr>
                                            <td style="width: 399px">
                                                <dx:ASPxLabel ID="lbl_SrcInsHeader" Text="Source Instance" Font-Bold="true" runat="server"></dx:ASPxLabel>
                                            </td>
                                            <td style="width: 629px">
                                                <dx:ASPxComboBox ID="ddlSourceIns" runat="server" DataSourceID="ds_ddlSourceIns" SelectedIndex="0" ValueType="System.String" TextField="Value">
                                                </dx:ASPxComboBox>
                                            </td>
                                            <td style="width: 399px">
                                                <dx:ASPxLabel ID="lbl_TgtInsHeader" Text="Target Instance" Font-Bold="true" runat="server"></dx:ASPxLabel>
                                            </td>
                                            <td style="width: 629px">
                                                <dx:ASPxComboBox ID="ddlTargetInstance" runat="server" DataSourceID="ds_ddlTargetIns" SelectedIndex="0" ValueType="System.String" TextField="Value">
                                                </dx:ASPxComboBox>
                                            </td>
                                            <td style="width: 416px">
                                                <dx:ASPxButton ID="btn_Submit" runat="server" OnClick="btn_Submit_Click" Text="Submit" AutoPostBack="False">
                                                </dx:ASPxButton>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td style="width: 399px">
                                                <dx:ASPxLabel ID="lbl_SubjecAreaHeader" runat="server" Text="Subject Area" Font-Bold="true">
                                                </dx:ASPxLabel>
                                            </td>
                                            <td style="width: 629px">
                                                <dx:ASPxComboBox ID="ddl_SubjecArea" runat="server" DataSourceID="ds_ddlSubArea" ValueType="System.String" TextField="Value" SelectedIndex="0" Enabled="false">
                                                </dx:ASPxComboBox>
                                            </td>
                                            <td style="width: 531px"></td>
                                            <td style="width: 629px">
                                                <asp:HiddenField ID="hdn_reqid" runat="server" />
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </fieldset>
                            <br />
                            <dx:ASPxLabel ID="lblMsg" runat="server" Font-Bold="False" Font-Size="8pt" ForeColor="#FF3300">
                            </dx:ASPxLabel>
                            <br />
                            <fieldset style="margin-left: auto; margin-right: auto;">
                                <legend class="Legend" style="font-size: 12px; color: #FF9933; font-weight: bold;">Request Keys</legend>
                                <dx:ASPxGridView ID="grd_Keys" Style="margin-left: auto; margin-right: auto;" SettingsBehavior-AllowSort="false" runat="server" DataSourceID="ds_grdkeys" AutoGenerateColumns="False" KeyFieldName="KEY1_VALUE" OnPageIndexChanged="grd_Keys_PageIndexChanged">
                                    <SettingsBehavior AllowSort="False" />
                                    <Columns>
                                        <dx:GridViewDataTextColumn FieldName="KEY1_COLUMN" ReadOnly="True" ShowInCustomizationForm="False" Visible="false" VisibleIndex="0">
                                        </dx:GridViewDataTextColumn>
                                        <dx:GridViewDataTextColumn FieldName="KEY1_VALUE" ReadOnly="True" ShowInCustomizationForm="True" VisibleIndex="1" Caption="PDBC_PFX">
                                        </dx:GridViewDataTextColumn>
                                    </Columns>
                                    <Styles>
                                        <Header Font-Bold="True">
                                        </Header>
                                        <Cell HorizontalAlign="Center" VerticalAlign="Middle">
                                        </Cell>
                                    </Styles>
                                </dx:ASPxGridView>
                            </fieldset>
                            <br />


                            <br />
                            <asp:SqlDataSource ID="SqlDataSource3" runat="server" ConnectionString="<%$ ConnectionStrings:BsSqlConnectionString %>" SelectCommand="PRY.selReqErrors" SelectCommandType="StoredProcedure">
                                <SelectParameters>
                                    <asp:Parameter Name="RequestID" Type="Int32" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                            <br />
                            <dx:ASPxGridView ID="grd_MigrationRequest" Style="margin-left: auto; margin-right: auto;" ClientInstanceName="grd_MigrationRequest" SettingsBehavior-AllowSort="false" runat="server" AutoGenerateColumns="False" DataSourceID="ds_grd_MigrationRequest" KeyFieldName="REQUEST_ID" OnPageIndexChanged="grd_MigrationRequest_PageIndexChanged">
                                <SettingsPager>
                                    <PageSizeItemSettings Visible="True">
                                    </PageSizeItemSettings>
                                </SettingsPager>
                                <SettingsBehavior AllowSort="False"></SettingsBehavior>
                                <Columns>
                                    <dx:GridViewDataTextColumn FieldName="REQUEST_ID" ReadOnly="True" ShowInCustomizationForm="True" Visible="false" VisibleIndex="0">
                                    </dx:GridViewDataTextColumn>
                                    <dx:GridViewDataTextColumn FieldName="RUNTIME_CTL" ReadOnly="True" ShowInCustomizationForm="True" VisibleIndex="1">
                                        <DataItemTemplate>
                                            <dx:ASPxHyperLink ID="hyp_run_ctl_id" runat="server" OnInit="hyp_run_ctl_id_Init">
                                            </dx:ASPxHyperLink>
                                        </DataItemTemplate>
                                    </dx:GridViewDataTextColumn>
                                    <dx:GridViewDataTextColumn FieldName="SRC_INSTANCE" ShowInCustomizationForm="True" VisibleIndex="2">
                                    </dx:GridViewDataTextColumn>
                                    <dx:GridViewDataTextColumn FieldName="TGT_INSTANCE" ShowInCustomizationForm="True" VisibleIndex="3">
                                    </dx:GridViewDataTextColumn>
                                    <dx:GridViewDataTextColumn FieldName="SUBJECT_AREA" ShowInCustomizationForm="True" VisibleIndex="4">
                                    </dx:GridViewDataTextColumn>
                                    <dx:GridViewDataTextColumn FieldName="TRACKING_NUMBER" ShowInCustomizationForm="True" VisibleIndex="5">
                                    </dx:GridViewDataTextColumn>
                                    <dx:GridViewDataTextColumn FieldName="USERNAME" ShowInCustomizationForm="True" VisibleIndex="6">
                                    </dx:GridViewDataTextColumn>
                                    <dx:GridViewDataDateColumn FieldName="CREATED_DTM" ShowInCustomizationForm="True" VisibleIndex="7">
                                    </dx:GridViewDataDateColumn>
                                </Columns>
                                <Styles>
                                    <Header Font-Bold="True">
                                    </Header>
                                    <Cell HorizontalAlign="Center" VerticalAlign="Middle">
                                    </Cell>
                                </Styles>
                            </dx:ASPxGridView>
                        </div>
                        <div id="dvTable2" runat="server">
                            <dx:ASPxLabel ID="lblErrMsg" runat="server">
                            </dx:ASPxLabel>
                        </div>
                        <asp:SqlDataSource ID="ds_grdkeys" runat="server" ConnectionString="<%$ ConnectionStrings:BsSqlConnectionString %>" SelectCommand="[BSBS].[mtReqKeyData]" SelectCommandType="StoredProcedure">
                            <SelectParameters>
                                <asp:Parameter Name="RequestID" Type="String" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                        <asp:SqlDataSource ID="ds_grd_MigrationRequest" runat="server" ConnectionString="<%$ ConnectionStrings:BsSqlConnectionString %>" SelectCommand="SELECT [REQUEST_ID],[RUNTIME_CTL],[SRC_INSTANCE],[TGT_INSTANCE],[SUBJECT_AREA],[TRACKING_NUMBER],[USERNAME],[CREATED_DTM] FROM [FS_MCD].[BSBS].[MigrationRequests] Where [REQUEST_ID]=@REQUEST_ID">
                            <SelectParameters>
                                <asp:Parameter Name="REQUEST_ID" Type="String" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                        <asp:SqlDataSource ID="ds_ddlSourceIns" runat="server" ConnectionString="<%$ ConnectionStrings:BsSqlConnectionString %>" SelectCommand="select [Value] from [FS_MCD].[BSBS].[MTConfig] where [Type]='SRC_INSTANCE'"></asp:SqlDataSource>
                        <asp:SqlDataSource ID="ds_ddlTargetIns" runat="server" ConnectionString="<%$ ConnectionStrings:BsSqlConnectionString %>" SelectCommand="select [Value] from [FS_MCD].[BSBS].[MTConfig] where [Type]='TGT_INSTANCE'"></asp:SqlDataSource>
                        <asp:SqlDataSource ID="ds_ddlSubArea" runat="server" ConnectionString="<%$ ConnectionStrings:BsSqlConnectionString %>" SelectCommand="select [Value] from [FS_MCD].[BSBS].[MTConfig] where [Type]='SUBJECT_AREA'"></asp:SqlDataSource>
                        <br />

                    </dx:PopupControlContentControl>
                </ContentCollection>
            </dx:ASPxPopupControl>
            <dx:ASPxPopupControl ID="ErrorCountStatus" Style="overflow: auto;" Target="_blank" Width="1050px" Height="550px" ClientInstanceName="ErrorCountStatus" PopupHorizontalAlign="WindowCenter" PopupVerticalAlign="WindowCenter" AllowResize="true" runat="server" ScrollBars="auto" CloseAnimationType="Auto" Theme="Default" OnWindowCallback="ErrorCountStatus_WindowCallback">
                <HeaderStyle Font-Bold="true" />
                <ContentCollection>
                    <dx:PopupControlContentControl runat="server">
                        <dx:ASPxGridView ID="errorDetails" DataSourceID="ds_ErrorCountStatus" runat="server" OnCustomCallback="errorgrid_CustomCallback" Style="margin-left: auto; margin-right: auto; " Width="100%"  AutoGenerateColumns="False"  ClientInstanceName="ErrorStatus" OnPageIndexChanged="errorDetails_PageIndexChanged" KeyFieldName="RequestID"  Theme="DevEx">
                            <Settings VerticalScrollBarMode="Visible" VerticalScrollableHeight="450" />    
                            <SettingsPager EnableAdaptivity="True" Mode="ShowAllRecords" >
                                <PageSizeItemSettings>
                                </PageSizeItemSettings>
                            </SettingsPager>
                            <SettingsBehavior AllowSort="False" />
                                 <SettingsDataSecurity AllowDelete="False" AllowEdit="False" AllowInsert="False" />
                            <Columns>
                                <dx:GridViewDataTextColumn FieldName="RequestID" VisibleIndex="0">
                                </dx:GridViewDataTextColumn>
                                <dx:GridViewDataTextColumn FieldName="Message" VisibleIndex="1">
                                </dx:GridViewDataTextColumn>
                                <dx:GridViewDataTextColumn FieldName="Description" VisibleIndex="2">
                                </dx:GridViewDataTextColumn>
                                <dx:GridViewDataTextColumn FieldName="RowNumber" VisibleIndex="3">
                                </dx:GridViewDataTextColumn>
                                <dx:GridViewDataTextColumn FieldName="ColumnChar" VisibleIndex="4">
                                </dx:GridViewDataTextColumn>
                                <dx:GridViewDataTextColumn FieldName="ActualValue" ShowInCustomizationForm="True" VisibleIndex="5">
                                </dx:GridViewDataTextColumn>
                            </Columns>
                            <Styles>
                                <Header Font-Bold="True">
                                </Header>
                                <Cell HorizontalAlign="Center" VerticalAlign="Middle">
                                </Cell>
                                
                                
                            </Styles>

                        </dx:ASPxGridView>
                        <br />
                        <asp:HiddenField ID="hdn_reqidpopup" runat="server" />
                        <br />
                        <asp:SqlDataSource ID="ds_ErrorCountStatus" runat="server" ConnectionString="<%$ ConnectionStrings:BsSqlConnectionString %>" SelectCommand="BSBS.selReqErrors" SelectCommandType="StoredProcedure">
                            <SelectParameters>
                                <asp:Parameter Name="RequestID" Type="Int32" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </dx:PopupControlContentControl>
                </ContentCollection>
            </dx:ASPxPopupControl>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
