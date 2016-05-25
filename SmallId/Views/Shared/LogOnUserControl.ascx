<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%
    if (Request.IsAuthenticated) {
%>
        Welcome <b><%= Html.Encode(Page.User.Identity.Name) %></b>
        (<%= SmallId.Code.Util.GetClaimedIdentifierForUser(Page.User.Identity.Name) %>)!
        [ <%= Html.ActionLink("Log Off", "LogOff", "Account") %> ]
<%
    }
    else {
%> 
        [ <%= Html.ActionLink("Log On", "LogOn", "Account") %> | <%= Html.ActionLink("Register", "Register", "Account") %> ]
<%
    }
%>
