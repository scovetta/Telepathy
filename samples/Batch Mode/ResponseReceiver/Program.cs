namespace Microsoft.Hpc.SOASample.BatchMode
{
    using System;

    using global::ResponseReceiver.ServiceReference;

    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Internal;

    class ResponseReceiver
    {
        static void Main(string[] args)
        {
            try
            {
                //Input sessionId here
                string sessionId;
                Console.Write("Input the session id : ");
                sessionId = Console.ReadLine();

                //Change the headnode name here
                SessionAttachInfo info = new SessionAttachInfo("head.contoso.com", sessionId);

                //Attach to session
                DurableSession session = DurableSession.AttachSession(info);
                Console.WriteLine("Attached to session {0}", sessionId);

                int numberResponse = 0;

                //Get responses
                using (BrokerClient<IPrimeFactorization> client = new BrokerClient<IPrimeFactorization>(session))
                {
                    foreach (BrokerResponse<FactorizeResponse> response in client.GetResponses<FactorizeResponse>())
                    {
                        int number = response.GetUserData<int>();
                        int[] factors = response.Result.FactorizeResult;

                        Console.WriteLine("{0} = {1}", number, string.Join<int>(" * ", factors));

                        numberResponse++;
                    }
                }

                session.Close(true);
                Console.WriteLine("{0} responses have been received", numberResponse);

                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
