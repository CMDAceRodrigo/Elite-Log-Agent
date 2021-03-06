﻿namespace DW.ELA.Plugin.EDSM
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;
    using DW.ELA.Interfaces;
    using DW.ELA.Utility;
    using NLog;

    internal class EdsmSettingsControl : AbstractSettingsControl
    {
        private TextBox edsmApiKeyTextBox;
        private Button testCredentialsButton;
        private Label credentialsStatusLabel;
        private Button button1;
        private CheckBox apiKeyValidatedCheckbox;
        private LinkLabel apiKeyLabel;
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public EdsmSettingsControl()
        {
            Load += EdsmSettingsControl_Load;
            InitializeComponent();
        }

        internal EdsmSettings Settings
        {
            get => new PluginSettingsFacade<EdsmSettings>(EdsmPlugin.CPluginId, GlobalSettings).Settings;
            set => new PluginSettingsFacade<EdsmSettings>(EdsmPlugin.CPluginId, GlobalSettings).Settings = value;
        }

        internal EdsmSettings ActualSettings
        {
            get
            {
                return new EdsmSettings()
                {
                    ApiKey = edsmApiKeyTextBox.Text,
                    Verified = apiKeyValidatedCheckbox.Checked
                };
            }

            set
            {
                edsmApiKeyTextBox.Text = value.ApiKey;
                apiKeyValidatedCheckbox.Checked = value.Verified;
            }
        }

        internal IRestClient RestClient { get; set; }

        private void EdsmSettingsControl_Load(object sender, EventArgs e) => ReloadSettings();

        private void ReloadSettings()
        {
            edsmApiKeyTextBox.Text = Settings.ApiKey;
            apiKeyValidatedCheckbox.Checked = Settings.Verified;
        }

        private void InitializeComponent()
        {
            button1 = new Button();
            edsmApiKeyTextBox = new TextBox();
            testCredentialsButton = new Button();
            credentialsStatusLabel = new Label();
            apiKeyValidatedCheckbox = new CheckBox();
            apiKeyLabel = new LinkLabel();
            SuspendLayout();

            // button1
            button1.Location = new Point(0, 0);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 0;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;

            // edsmApiKeyTextBox
            edsmApiKeyTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left
            | AnchorStyles.Right;
            edsmApiKeyTextBox.Location = new Point(87, 3);
            edsmApiKeyTextBox.Name = "edsmApiKeyTextBox";
            edsmApiKeyTextBox.Size = new Size(284, 20);
            edsmApiKeyTextBox.TabIndex = 1;
            edsmApiKeyTextBox.Text = "EDSM API Key";
            edsmApiKeyTextBox.TextAlign = HorizontalAlignment.Center;
            edsmApiKeyTextBox.TextChanged += new EventHandler(EdsmApiKeyTextBox_TextChanged);

            // testCredentialsButton
            testCredentialsButton.Location = new Point(3, 27);
            testCredentialsButton.Name = "testCredentialsButton";
            testCredentialsButton.Size = new Size(125, 23);
            testCredentialsButton.TabIndex = 2;
            testCredentialsButton.Text = "Validate";
            testCredentialsButton.UseVisualStyleBackColor = true;
            testCredentialsButton.Click += new EventHandler(TestCredentialsButton_Click);

            // credentialsStatusLabel
            credentialsStatusLabel.AutoSize = true;
            credentialsStatusLabel.Location = new Point(3, 55);
            credentialsStatusLabel.Name = "credentialsStatusLabel";
            credentialsStatusLabel.Size = new Size(69, 13);
            credentialsStatusLabel.TabIndex = 3;
            credentialsStatusLabel.Text = "Not checked";

            // apiKeyValidatedCheckbox
            apiKeyValidatedCheckbox.AutoSize = true;
            apiKeyValidatedCheckbox.Enabled = false;
            apiKeyValidatedCheckbox.Location = new Point(134, 31);
            apiKeyValidatedCheckbox.Name = "apiKeyValidatedCheckbox";
            apiKeyValidatedCheckbox.Size = new Size(184, 17);
            apiKeyValidatedCheckbox.TabIndex = 4;
            apiKeyValidatedCheckbox.Text = "CMDR Name / API Key validated";
            apiKeyValidatedCheckbox.UseVisualStyleBackColor = true;
            apiKeyValidatedCheckbox.CheckedChanged += new EventHandler(ApiKeyValidatedCheckbox_CheckedChanged);

            // apiKeyLabel
            apiKeyLabel.AutoSize = true;
            apiKeyLabel.Location = new Point(3, 6);
            apiKeyLabel.Name = "apiKeyLabel";
            apiKeyLabel.Size = new Size(81, 13);
            apiKeyLabel.TabIndex = 6;
            apiKeyLabel.TabStop = true;
            apiKeyLabel.Text = "EDSM API key:";
            apiKeyLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(ApiKeyLabel_LinkClicked);

            // EdsmSettingsControl
            Controls.Add(apiKeyLabel);
            Controls.Add(apiKeyValidatedCheckbox);
            Controls.Add(credentialsStatusLabel);
            Controls.Add(testCredentialsButton);
            Controls.Add(edsmApiKeyTextBox);
            Name = "EdsmSettingsControl";
            Size = new Size(374, 150);
            ResumeLayout(false);
            PerformLayout();
        }

        private async void TestCredentialsButton_Click(object sender, EventArgs e)
        {
            try
            {
                testCredentialsButton.Enabled = false;
                var apiKey = edsmApiKeyTextBox.Text;
                var apiFacade = new EdsmApiFacade(new ThrottlingRestClient.Factory().CreateRestClient("https://www.edsm.net/api-commander-v1/get-ranks"), GlobalSettings.CommanderName, apiKey);
                var result = await apiFacade.GetCommanderRanks();
                if (result["ranksVerbose"] == null)
                    throw new EdsmApiException("No access to CMDR ranks");
                credentialsStatusLabel.Text = "Success, combat rank: " + result["ranksVerbose"]["Combat"]?.ToString();
                apiKeyValidatedCheckbox.Checked = true;
            }
            catch (Exception ex)
            {
                credentialsStatusLabel.Text = ex.Message;
                apiKeyValidatedCheckbox.Checked = false;
            }
            finally
            {
                testCredentialsButton.Enabled = true;
            }
        }

        public override void SaveSettings() => Settings = new EdsmSettings() { ApiKey = edsmApiKeyTextBox.Text, Verified = apiKeyValidatedCheckbox.Checked };

        private void ApiKeyLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.edsm.net/en/settings/api");
        }

        private void EdsmApiKeyTextBox_TextChanged(object sender, EventArgs e)
        {
            edsmApiKeyTextBox.BackColor = Color.LightGray;
        }

        private void ApiKeyValidatedCheckbox_CheckedChanged(object sender, EventArgs e) => edsmApiKeyTextBox.BackColor = apiKeyValidatedCheckbox.Checked ? Color.LightGreen : Color.LightSalmon;
    }
}
