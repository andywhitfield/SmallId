﻿<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="scriptHeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        $(function () {
            var usernameEl = $('input[name="username"]');
            if (usernameEl.val() != '') {
                $('input[name="password"]').focus();
            } else {
                usernameEl.focus();
            }
        });
    </script>
</asp:Content>

<asp:Content ID="loginContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Log On</h2>
    <p>
        Please enter your username and password.
    </p>
    <%= Html.ValidationSummary("Login was unsuccessful. Please correct the errors and try again.") %>

    <% using (Html.BeginForm()) { %>
        <div>
            <fieldset>
                <legend>Account Information</legend>
                <p>
                    <label for="username">Username:</label>
                    <%= Html.TextBox("username", TempData["username"]) %>
                    <%= Html.ValidationMessage("username") %>
                </p>
                <p>
                    <label for="password">Password:</label>
                    <%= Html.Password("password") %>
                    <%= Html.ValidationMessage("password") %>
                </p>
                <p>
                    <%= Html.CheckBox("rememberMe") %> <label class="inline" for="rememberMe">Remember me?</label>
                </p>
                <p>
                    <input type="submit" value="Log On" />
                </p>
            </fieldset>
        </div>
    <% } %>
</asp:Content>
