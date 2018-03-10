using Amazon.EC2;
//using NetFwTypeLib;
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
        //public string GetPublicIpAddress()
        //{
        //    try
        //    {

        //        // Use the .NET sdk meta-data to retrieve the public ip address
        //        AmazonEC2Client myInstance = new AmazonEC2Client();
        //        Amazon.EC2.Model.DescribeInstancesRequest request = new Amazon.EC2.Model.DescribeInstancesRequest();
        //        request.InstanceIds.Add(Amazon.Util.EC2InstanceMetadata.InstanceId);
        //        Amazon.EC2.Model.DescribeInstancesResponse response = myInstance.DescribeInstances(request);

        //        instance.IpAddress = response.Reservations[0].Instances[0].PublicIpAddress;
        //    }
        //    catch (AmazonEC2Exception e)
        //    {
        //        Console.WriteLine(e);
        //    }

        //    return instance.IpAddress;
        //}
    }
}
