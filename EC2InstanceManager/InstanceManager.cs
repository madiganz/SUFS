using Amazon.EC2;
using NetFwTypeLib;
using System;
using System.Collections.Generic;

namespace EC2InstanceManager
{
    public class InstanceManager
    {

        private static InstanceManager instance;

        public string IpAddress { get; private set; }

        /// <summary>
        /// Initialize the instance manager
        /// </summary>
        private InstanceManager()
        {

        }

        /// <summary>
        /// Singleton pattern to force only 1 instance of instance manager
        /// </summary>
        public static InstanceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new InstanceManager();
                }
                return instance;
            }
        }

        /// <summary>
        /// Gets the EC2 instances private ip address
        /// </summary>
        /// <returns></returns>
        public string GetPrivateIpAddress()
        {
            try
            {
                return Amazon.Util.EC2InstanceMetadata.PrivateIpAddress;
            }
            catch(Exception e)
            {
                Console.WriteLine("Failed to get privateIpAddress, " + e);
                return "localhost";
            }
        }

        /// <summary>
        /// Gets the EC2 instances public ip address
        /// </summary>
        public string GetPublicIpAddress()
        {
            try
            {

                // Use the .NET sdk meta-data to retrieve the public ip address
                AmazonEC2Client myInstance = new AmazonEC2Client();
                Amazon.EC2.Model.DescribeInstancesRequest request = new Amazon.EC2.Model.DescribeInstancesRequest();
                request.InstanceIds.Add(Amazon.Util.EC2InstanceMetadata.InstanceId);
                Amazon.EC2.Model.DescribeInstancesResponse response = myInstance.DescribeInstances(request);

                instance.IpAddress = response.Reservations[0].Instances[0].PublicIpAddress;
            }
            catch (AmazonEC2Exception e)
            {
                Console.WriteLine(e);
            }

            return instance.IpAddress;
        }

        /// <summary>
        /// Opens a port on the firewall for both inbound and outbound traffic
        /// </summary>
        /// <param name="port">Port for rule</param>
        public void OpenFirewallPort(string port)
        {
            if (!FirewallRuleExists())
            {
                CreateFirewallRule(port, "inbound");
            }
            if (!FirewallRuleExists(NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT))
            {
                CreateFirewallRule(port, "outbound");
            }
        }

        /// <summary>
        /// Creates a firewall rule for a port
        /// </summary>
        /// <param name="port">Port for rule</param>
        /// <param name="direction">inbound or outbound</param>
        private void CreateFirewallRule(string port, string direction)
        {
            try
            {
                // Create inbound and output rule
                INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                firewallRule.Description = "Allows Grpc over ec2 instances";
                if (direction == "inbound")
                {
                    firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                }
                else if (direction == "outbound")
                {
                    firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
                }
                firewallRule.Enabled = true;
                firewallRule.InterfaceTypes = "All";
                firewallRule.Name = "GRPC";
                firewallRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
                firewallRule.LocalPorts = port;
                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                    Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                firewallPolicy.Rules.Add(firewallRule);
            }
            catch(Exception e)
            {
                Console.WriteLine("Creating firewall rule failed, " + e);
            }
        }

        /// <summary>
        /// Check if firewall rule already exists
        /// </summary>
        /// <param name="direction">Direction of rule. Defaulted to inbound</param>
        /// <param name="RuleName">Rule name. Defaulted to GRPC</param>
        /// <returns></returns>
        private static bool FirewallRuleExists(NET_FW_RULE_DIRECTION_ direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN, string name = "GRPC")
        {
            try
            {
                Type tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                INetFwPolicy2 fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(tNetFwPolicy2);
                var currentProfiles = fwPolicy2.CurrentProfileTypes;

                List<INetFwRule> RuleList = new List<INetFwRule>();

                foreach (INetFwRule rule in fwPolicy2.Rules)
                {
                    if (rule.Name.IndexOf(name) != -1 && rule.Direction == direction)
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error checking if firewall rule exists, " + e);
            }

            return false;
        }
    }
}
