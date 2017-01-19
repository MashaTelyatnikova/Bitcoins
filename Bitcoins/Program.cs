using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Nancy.Json;

namespace Bitcoins
{
    public static class BigIntegerExtensions
    {
        public static BigInteger Mode(this BigInteger a, BigInteger modular)
        {
            var result = a%modular;
            if (result < 0)
            {
                result += modular;
            }
            return result;
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            var mode = int.Parse(args[0]);

            var repository =
                new JavaScriptSerializer().Deserialize<UserRepository>(File.ReadAllText("1.json"));

            switch (mode)
            {
                case 0:
                {
                    var userId = args[1];
                    var user = repository.Users.FirstOrDefault(y => y.Id == userId);
                    if (user == null)
                    {
                        Console.WriteLine("User not found.");
                        Environment.Exit(0);
                    }

                    var lastNotConfirmedBlock = user.Blocks.LastOrDefault(y => !y.IsConfirmed);
                    if (lastNotConfirmedBlock == null)
                    {
                        lastNotConfirmedBlock = new Block();
                        lastNotConfirmedBlock.Transactions = new List<Transaction>();
                    }
                    //todo mining
                    var msg = Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(lastNotConfirmedBlock));
                    var result = DoTheMining(msg);
                    var hash = Encoding.UTF8.GetString(result.Item1);
                    lastNotConfirmedBlock.Counter = result.Item2;
                    lastNotConfirmedBlock.Id = hash;

                    var transaction = new Transaction()
                    {
                        //Типо если нет рефа, то значит сам намайнил, проверяем хэш
                        Id = Guid.NewGuid().ToString(),
                        SourceUserId = userId,
                        Sum = 25,
                    };
                    transaction.Hash =
                        Encoding.UTF8.GetString(
                            GetMd5Hash(
                                Encoding.UTF8.GetBytes(transaction.Id + transaction.SourceUserId + transaction.Sum)));
                    lastNotConfirmedBlock.Transactions.Add(transaction);
                    //lastNotConfirmedBlock.IsConfirmed = true;
                    user.Blocks.Add(lastNotConfirmedBlock);

                    foreach (var user1 in repository.Users)
                    {
                        if (user1.Blocks.All(y => y.IsConfirmed))
                        {
                            user1.Blocks.Add(lastNotConfirmedBlock);
                        }
                    }

                    for (var i = 0; i < repository.Users.Count - 1; ++i)
                    {
                        for (var j = i + 1; j < repository.Users.Count; ++j)
                        {
                            var first = repository.Users[i];
                            var second = repository.Users[j];

                            var firstLast = first.Blocks.LastOrDefault(y => !y.IsConfirmed);
                            var secondLast = second.Blocks.LastOrDefault(y => !y.IsConfirmed);
                            if (firstLast != null && secondLast != null)
                            {
                                if (firstLast.Transactions.Count >= secondLast.Transactions.Count)
                                {
                                    firstLast.IsConfirmed = true;
                                    second.Blocks.Remove(secondLast);
                                    second.Blocks.Add(firstLast);
                                }
                                else
                                {
                                    secondLast.IsConfirmed = true;
                                    first.Blocks.Remove(firstLast);
                                    first.Blocks.Add(secondLast);
                                }
                            }
                        }
                    }


                    File.WriteAllText("1.json", new JavaScriptSerializer().Serialize(repository));
                    break;
                }
                case 1:
                {
                    var source = args[1];
                    var target = args[2];
                    var sum = long.Parse(args[3]);

                    var sourceUser = repository.Users.FirstOrDefault(y => y.Id == source);
                    if (sourceUser == null)
                    {
                        Console.WriteLine("Source user doesn't exist");
                        Environment.Exit(0);
                    }

                    var targetUser = repository.Users.FirstOrDefault(y => y.Id == target);
                    if (targetUser == null)
                    {
                        Console.WriteLine("Target user doesn't exist");
                        Environment.Exit(0);
                    }
                    var newTr = new Transaction();
                    newTr.Id = new Guid().ToString();
                    var lastTr = sourceUser.Blocks.LastOrDefault()?.Transactions.LastOrDefault();
                    newTr.RefTransactionId = lastTr?.Id;
                    newTr.SourceUserId = target;
                    newTr.Sum = sum;
                    var val = GetMd5Hash(Encoding.UTF8.GetBytes($"{lastTr.Hash}{newTr.Id}{target}{sum}"));
                    newTr.Hash = Encoding.UTF8.GetString(val);
                    var data = new BigInteger(val);
                    data = data.Mode(sourceUser.P);
                    var result = BigInteger.ModPow(data, sourceUser.D, sourceUser.P);
                    newTr.Signature = result.ToString();

                    foreach (var user in repository.Users)
                    {
                        var firstLast = user.Blocks.LastOrDefault(y => !y.IsConfirmed);
                        if (firstLast == null)
                        {
                            firstLast = new Block() {Transactions = new List<Transaction>()};
                            user.Blocks.Add(firstLast);
                        }
                        firstLast.Transactions.Add(newTr);
                    }

                    File.WriteAllText("1.json", new JavaScriptSerializer().Serialize(repository));
                    break;
                }
            }
        }

        private static Tuple<byte[], byte[]> DoTheMining(byte[] msg)
        {
            var counter = new byte[2];

            byte[] hash = GetMd5Hash(msg.Concat(counter).ToArray());

            while (hash[0] != 0 || hash[1] != 0)
            {
                for (var i = 0; i < counter.Length; i++)
                {
                    counter[i]++;
                    if (counter[i] != 0)
                        break;
                }
                hash = GetMd5Hash(msg.Concat(counter).ToArray());
            }

            return Tuple.Create(hash, counter);
        }

        public static byte[] GetMd5Hash(byte[] input)
        {
            using (var md5Hash = MD5.Create())
            {
                return md5Hash.ComputeHash(input);
            }
        }
    }
}