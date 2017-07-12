﻿using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.DataStructures.ServerModel;
using NitroxModel.Packets;
using System;
using UnityEngine;
using NitroxClient.GameLogic.Helper;

namespace NitroxClient.Communication
{
    public class PacketSender
    {
        public bool Active { get; set; }
        public String PlayerId { get; set; }

        private TcpClient client;
        
        public PacketSender(TcpClient client)
        {
            this.client = client;
            this.Active = false;
        }

        public void Authenticate()
        {
            Authenticate auth = new Authenticate(PlayerId);
            Send(auth);
        }

        public void UpdatePlayerLocation(Vector3 location, Quaternion rotation, Optional<VehicleModel> opVehicle)
        {
            Movement movement;

            if (opVehicle.IsPresent())
            {
                VehicleModel vehicle = opVehicle.Get();
                movement = new VehicleMovement(PlayerId, ApiHelper.Vector3(location), vehicle.Rotation, vehicle.TechType, vehicle.Guid);
            }
            else
            {
                movement = new Movement(PlayerId, ApiHelper.Vector3(location), ApiHelper.Quaternion(rotation));
            }

            Send(movement);
        }

        public void UpdateItemPosition(String guid, Vector3 location, Quaternion rotation)
        {
            ItemPosition itemPosition = new ItemPosition(PlayerId, guid, ApiHelper.Vector3(location), ApiHelper.Quaternion(rotation));            
            Send(itemPosition);
        }

        public void PickupItem(GameObject gameObject, String techType)
        {
            String guid = GuidHelper.GetGuid(gameObject);
            Vector3 itemPosition = gameObject.transform.position;

            PickupItem pickupItem = new PickupItem(PlayerId, ApiHelper.Vector3(itemPosition), guid, techType);
            Send(pickupItem);
        }

        public void DropItem(GameObject gameObject, TechType techType, Vector3 dropPosition)
        {
            String guid = GuidHelper.GetGuid(gameObject);
            SyncedMultiplayerObject.ApplyTo(gameObject);
            
            Console.WriteLine("Dropping item with guid: " + guid);

            DroppedItem droppedItem = new DroppedItem(PlayerId, guid, ApiHelper.TechType(techType), ApiHelper.Vector3(dropPosition));
            Send(droppedItem);
        }

        public void BuildItem(String techType, Vector3 itemPosition, Quaternion quaternion)
        {
            BeginItemConstruction buildItem = new BeginItemConstruction(PlayerId, ApiHelper.Vector3(itemPosition), ApiHelper.Quaternion(quaternion), techType);
            Send(buildItem);
        }

        public void ChangeConstructionAmount(Vector3 itemPosition, float amount, int resourceId1, int resourceId2)
        {
            if (amount >= 1f || resourceId1 != resourceId2)
            {
                ConstructionAmountChanged amountChanged = new ConstructionAmountChanged(PlayerId, ApiHelper.Vector3(itemPosition), amount);
                Send(amountChanged);
            }
        }
        
        public void ConstructorBeginCrafting(GameObject constructor, TechType techType, float duration)
        {
            String guid = GuidHelper.GetGuid(constructor);

            Console.WriteLine("Building item from constructor with uuid: " + guid);

            ConstructorBeginCrafting beginCrafting = new ConstructorBeginCrafting(PlayerId, guid, ApiHelper.TechType(techType), duration);
            Send(beginCrafting);
        }

        public void SendChatMessage(String text)
        {
            ChatMessage message = new ChatMessage(PlayerId, text);
            Send(message);
        }

        public void AnimationChange(AnimChangeType type, AnimChangeState state)
        {
            AnimationChangeEvent animEvent = new AnimationChangeEvent(PlayerId, (int)type, (int)state);
            Send(animEvent);
        }

        public void Send(Packet packet)
        {
            if (Active)
            {
                try
                {
                    client.Send(packet);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error sending packet " + packet, ex);
                }
            }
        }
    }
}
