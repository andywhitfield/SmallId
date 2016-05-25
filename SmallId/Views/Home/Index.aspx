<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Register Assembly="DotNetOpenAuth.OpenId.UI" Namespace="DotNetOpenAuth" TagPrefix="openauth" %>
<asp:Content runat="server" ContentPlaceHolderID="HeadContent">
	<openauth:XrdsPublisher ID="XrdsPublisher1" runat="server" XrdsUrl="~/Home/xrds"
		XrdsAutoAnswer="false" />
</asp:Content>
<asp:Content ID="indexMain" ContentPlaceHolderID="MainContent" runat="server">
<%
    if (Request.IsAuthenticated) {
%>
        Welcome <b><%= Html.Encode(Page.User.Identity.Name) %></b>!
        <%= Html.ActionLink("Log Off", "LogOff", "Account") %>
<%
    }
    else {
%> 
        Welcome to SmallId - <%= Html.ActionLink("Log On", "LogOn", "Account") %>
        or <%= Html.ActionLink("Register", "Register", "Account") %>.
<%
    }
%>
</asp:Content>