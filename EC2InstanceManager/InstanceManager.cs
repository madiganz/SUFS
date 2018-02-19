using Amazon.EC2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EC2InstanceManager
{
    public class InstanceManager
    {

        private static InstanceManager instance;

        public string ipAddress { get; private set; }

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
            return Amazon.Util.EC2InstanceMetadata.PrivateIpAddress;
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

                instance.ipAddress = response.Reservations[0].Instances[0].PublicIpAddress;
            }
            catch (AmazonEC2Exception e)
            {
                Console.WriteLine(e);
            }

            return instance.ipAddress;
        }
    }
}
