<%@ Page Title="" Language="C#" MasterPageFile="~/Root.master" AutoEventWireup="true" CodeBehind="ExcelToCsvExtract.aspx.cs" Inherits="FeeScheduleManager.UI.ExcelToCsvExtract" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
<%@ Register Assembly="FeeScheduleManager" Namespace="FeeScheduleManager.Controls" TagPrefix="agp" %>
<%@ Register Src="~/Controls/AgpJModal.ascx" TagPrefix="uc" TagName="JModal" %>
<%@ Register Assembly="DevExpress.Web.v17.2, Version=17.2.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="Content" runat="server">
    
    <asp:ScriptManager ID="ScriptManager1" runat="server" AsyncPostBackTimeout ="360000">
    </asp:ScriptManager>
    <script>
        
        function fnCallback() {

            document.getElementById("<%=BtnConvert.ClientID %>").disabled = '';
        }
        function showFileUpload() {
            var btn = document.getElementById("<%=FileUpload1.ClientID %>");
            btn.click();
        }
        function radioButton2_CheckedChanged() {
            var btn = document.getElementById("<%=TxtStartRow.ClientID %>");
            btn.removeAttribute(disabled) = true;
           
        }
        function radioButton1_CheckedChanged() {
            var btn = document.getElementById("<%=TxtStartRow.ClientID %>").disabled='';
             
        }
        function UploadFile(fileUpload) {
            if (fileUpload.value != '') {
                document.getElementById("<%=BtnAdd.ClientID %>").click();
             }
         }
       
       
       
    </script>
    <style>
           .tbl{
           text-align:center;
       
           min-height:fit-content;
           vertical-align:central;
          margin:1px;
       }
        .auto-style3 {
            width: 25px;
        }
        .auto-style4 {
            width: 491px;
        }
        .auto-style5 {
            visibility: hidden;
        }
        </style>
        
        <ContentTemplate >
      <fieldset style="margin: 5% auto 0 auto; width: 1000px;height:fit-content">
                <legend class="Legend" style="font-size: 12px; color: #FF9933; font-weight: bold;">Fee Schedule Data Excel File</legend>
           <table id="tbl" style="margin: 20px ; width: 1120px;text-align:center;vertical-align:central;">
               <tr>
                   <td class="auto-style3">

                   </td>
                   <td>

                   </td>
                   <td class="auto-style4">
                       <asp:ListBox ID="ListBox1" runat="server" Rows="5" SelectionMode="Single" Width="350px" BackColor="White"></asp:ListBox>
                   </td>
                   <td style="text-align:left">
                       <asp:Button ID="BtnRemove" runat="server" Text="Remove" Width="101px" OnClick="BtnRemove_Click" CssClass="btn btn-secondary" BackColor="White" Visible="true" />
                   </td>
                   
               </tr>
               <tr style="height:10px">
                   <td class="auto-style3"></td>
               </tr>
                    
                    <tr>
                        <td class="auto-style3" ></td>
                        <td class="dxeTextBoxDefaultWidthSys" style="width: 160px; height: 5px">Fee Schedule Excel File&nbsp; *</td>
                        <td class="auto-style4">
                            <asp:textbox ID="txtFileLocation"  runat="server" borderstyle="outset" height="30px" width="90%"></asp:textbox>
                                                     
                        </td>
                        <td style="text-align:left">
                               <label id="btnlbl" onclick="showFileUpload()" for="FileUpload1" title="FileUpload" style="cursor: pointer; padding:8px 15px 8px 15px;border:1px solid black"><b>FileUpload</b>  </label>
                           <asp:FileUpload ID="FileUpload1" runat="server" AllowMultiple="True"  Width="16px" CssClass="auto-style5" />
                      
                        </td>
                    </tr>
                <tr style="height:15px">
                   <td class="auto-style3"></td>
               </tr>
                    <tr>
                        <td class="auto-style3" >
                        </td>
                        <td></td>
                        <td class="auto-style4">
                       <asp:Button ID="BtnAdd" runat="server" Text="Add" Width="103px" OnClick="BtnAdd_Click"  BackColor="White" CssClass="auto-style5" />
                                      
                   </td>
                    </tr>
                
                    <tr>
                        <td class="auto-style3" ></td>
                        <td>
                             <span style="font-style: normal; font-weight: bold" >Message : </span> 
                        </td>
                        <td class="auto-style4">
                            <asp:Label ID="LblMsg" runat="server" Text="Label" ForeColor="Black"></asp:Label>
                              
                        </td>
                        <td>
                            
                        </td>
                    </tr>
               
                    <tr style="visibility:hidden">
                        <td class="auto-style3" >
                        </td>
                        <td></td>
                        <td class="auto-style4">
                       <asp:RadioButton ID="RadioButton1" runat="server" GroupName="radiobtngrp" Text="Export From Row-0"   OnCheckedChanged="RadioButton1_CheckedChanged"  AutoPostBack="true" Visible="false" />
                       <asp:RadioButton ID="RadioButton2" runat="server" GroupName="radiobtngrp" Text="Exprot From Row-" OnCheckedChanged="RadioButton2_CheckedChanged"   AutoPostBack="true"  Visible="false"></asp:RadioButton>
                       <asp:TextBox ID="TxtStartRow" runat="server" Height="30px" Width="46px" ></asp:TextBox>
                        </td>
                        <td style="text-align:left">
                <asp:RangeValidator ID="RangeValidator1" runat="server" ErrorMessage="Row Number must be Above the 1 to 100 " ControlToValidate="TxtStartRow" ForeColor="Red" MaximumValue="1000" MinimumValue="1" Type="Integer"></asp:RangeValidator>
                        </td>
                    </tr>
               
                    <tr style="height:20px">
                        <td class="auto-style3"></td>
                        <td>

                        </td>
                        <td style="text-align:left">
                            <asp:CheckBox ID="chkUseSheetNames" runat="server" Text="Name CSVs using sheet names (not indexes, sheet&lt;'n'&gt;)" BackColor="White" BorderColor="White" Checked="True" Font-Bold="False" />
                        </td>
                    </tr>
                <tr style="height:15px">
                   <td class="auto-style3"></td>
               </tr>
               <tr style="height:35px">
                   <td class="auto-style3"></td>
                        
                   <td></td>
                           <td style="text-align:left">
                               <asp:CheckBox ID="cboxSpecialFormatting" runat="server"  Text="Use raw formatting?"/>
                           </td>

               </tr>
             <tr style="height:35px">
                   <td class="auto-style3"></td>
                   <td></td>
               </tr>
                    <tr style="height:35px">
                        <td class="auto-style3" >
                        </td>
                        <td></td>
                        <td class="auto-style4">
                      <asp:Button ID="BtnConvert" runat="server" Text="Convert-To-CSV" Width="340px" OnClick="BtnConvert_Click"  CssClass="btn btn-info" Font-Bold="True" BackColor="White" Height="36px" />
                        </td>
                        <td>
                          </td>
                    </tr>
                    </table>
          </fieldset>
            </ContentTemplate>
       
 
</asp:Content>

