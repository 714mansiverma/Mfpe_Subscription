﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SubscriptionService.Models;
using SubscriptionService.Controllers;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;

namespace SubscriptionService.Repository
{
    public class SubscribeDrugs : ISubscribeDrugs
    {
        static List<SubscriptionDetails> details;
        static readonly log4net.ILog _log4net = log4net.LogManager.GetLogger(typeof(SubscribeController));
        public SubscribeDrugs()
        {
            details = new List<SubscriptionDetails>() {
                new SubscriptionDetails{ Id=1, MemberId=201, MemberLocation="Delhi", PrescriptionId=101, RefillOccurrence="weekly", Status=true, SubscriptionDate= Convert.ToDateTime("2020-12-01 01:01:00 AM")},
                new SubscriptionDetails{ Id=2, MemberId=202, MemberLocation="Kolkata", PrescriptionId=102, RefillOccurrence="Monthly", Status=true, SubscriptionDate= Convert.ToDateTime("2020-12-01 01:01:00 AM")},
                 new SubscriptionDetails{ Id=3, MemberId=303, MemberLocation="Kolkata", PrescriptionId=103, RefillOccurrence="Monthly", Status=true, SubscriptionDate= Convert.ToDateTime("2020-12-01 01:01:00 AM")}
             };
        }
        public SubscriptionDetails PostSubscription(PrescriptionDetails prescription, string PolicyDetails, int Member_Id)
        {
            _log4net.Info("DruApi is being called to check for the availability of the DrugName= "+prescription.DrugName);
            // Drug drug = new Drug() { DrugId = 1, EpiryDate = new DateTime(1999, 12, 20), Id = 1, ManufactureDate = Convert.ToDateTime("2020-12-01 01:01:00 AM"), ManufacturerName = "XYZ", Name = "Paracetamol" };
            List<LocationWiseDrug> location = new List<LocationWiseDrug>();
              var drugs = "";
              var query = prescription.DrugName;
              HttpClient client = new HttpClient();
            HttpResponseMessage result=null;
            try
            {
                result = client.GetAsync("https://localhost:44393/api/DrugsApi/searchDrugsByName/" + query).Result;
            }
            catch (Exception ex)
            {
                _log4net.Error("Exception occured in calling Drug Api" + nameof(SubscribeDrugs) + " and error is" + ex.Message);
            }
            

              if (result.IsSuccessStatusCode)
              {
                  drugs = result.Content.ReadAsStringAsync().Result;
                location= JsonConvert.DeserializeObject<List<LocationWiseDrug>>(drugs);

            }
            if (location.Count!=0)
            {
                _log4net.Info(prescription.DrugName+" Drug Available");
                var last = details.Last();
                SubscriptionDetails subscription= new SubscriptionDetails { Id = (last.Id+1), MemberId = Member_Id, MemberLocation = "Delhi", PrescriptionId = prescription.Id, RefillOccurrence = prescription.RefillOccurrence, Status = true, SubscriptionDate = DateTime.Now };
                
                return subscription;
            }
            else
            {
                _log4net.Info(prescription.DrugName+" Drug NotAvailable");
                return new SubscriptionDetails { Id = 0, MemberId = 0, MemberLocation = "", PrescriptionId = 0, RefillOccurrence = "", Status = false, SubscriptionDate = Convert.ToDateTime("2020-12-01 01:01:00 AM") };
            }
        }
        public SubscriptionDetails PostUnSubscription(int Member_Id, int Subscription_Id)
        {

            // Get the data from refill microservice 
            _log4net.Info("Checking for Subscriptionid= "+Subscription_Id);
            
                SubscriptionDetails result = new SubscriptionDetails();
                var subs = details.Find(p => p.Id == Subscription_Id);
                if (subs != null)
                {
                    _log4net.Info("Interacting with refill microservice for the payment status for subscription id =" + Subscription_Id);

               

                    using (var httpClient = new HttpClient())
                    {
                        using (var response = httpClient.GetAsync("https://localhost:44365/api/RefillOrders/RefillDues/" + Subscription_Id).Result)
                        {

                            if (!response.IsSuccessStatusCode)
                            {
                                return result;
                            }

                            var data = response.Content.ReadAsStringAsync().Result;

                            var due = JsonConvert.DeserializeObject<int>(data);

                            result = subs;

                            _log4net.Info("Number of Refills pending for payment" + due + "for subscriptionId" + Subscription_Id);


                            if (due == 0)
                            {
                                var unsubscribe = details.Find(p => p.Id == Subscription_Id);
                                result.Status = false;
                                details.Remove(unsubscribe);
                            }

                            
                        }
                    }
                             return result;
                }
                return null;
            
        }
    }
}