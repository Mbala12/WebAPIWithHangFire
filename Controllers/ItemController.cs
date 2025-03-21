using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebAPIWithHangFire.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly AppDbContext conn;
        public ItemController(AppDbContext conn)
        {
            this.conn = conn;
        }

        //Background Jobs
        //Display message(object) As soon as post is made
        [HttpPost("BackgroundJob-Enqueue")]
        public async Task<IActionResult> Post([FromBody] Item item)
        {
            conn.Items.Add(item);
            await conn.SaveChangesAsync();
            BackgroundJob.Enqueue(() => DisplayBackground(item));
            return Ok(true);
        }
        public static void DisplayBackground(Item item)
        {
            Console.WriteLine(Environment.NewLine); 
            Console.WriteLine("This is the item added");
            Console.WriteLine("Name: {0}", item.Name);
            Console.WriteLine("Quantity: {0} {1}", item.Qty, Environment.NewLine);
        }

        //Schedule Jobs
        //Display List of items 8 seconds past after calling this method.

        [HttpGet("Background-Schedule")]
        public async Task<IActionResult> Get()
        {
            var items = await conn.Items.ToListAsync();
            BackgroundJob.Schedule(() => DoConsoleStuff(items), new DateTimeOffset(DateTime.UtcNow.AddSeconds(5)));
            return Ok(items);
        }

        public static void DoConsoleStuff(List<Item> items)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"[Name     Quantity]");
            foreach(var item in items)
            {
                Console.WriteLine($"[{item.Name} ----------- {item.Qty}]");
            }
            Console.WriteLine(Environment.NewLine);
        }

        //Continuation Job
        //Display items total quantity 10 iyems, it first needs started job's Id to check if it has finished then it continues.

        [HttpGet("BackgroundJob-ContinueJobWith")]
        public async Task<IActionResult> TotalQty()
        {
            var items = await conn.Items.ToListAsync();
            var DefaultJobId = BackgroundJob.Schedule(() => DoConsoleStuff(items), new DateTimeOffset(DateTime.UtcNow.AddSeconds(5)));
            var newjob1 = BackgroundJob.ContinueJobWith(DefaultJobId, () => GetItemQuantity(items));
            var newjob2 = BackgroundJob.ContinueJobWith(newjob1, () => GetItemQuantity(items));
            var newjob3 = BackgroundJob.ContinueJobWith(newjob2, () => GetItemQuantity(items));
            var newjob4 = BackgroundJob.ContinueJobWith(newjob3, () => GetItemQuantity(items));
            var newjob5 = BackgroundJob.ContinueJobWith(newjob4, () => GetItemQuantity(items));
            return Ok(items.Sum(_ => _.Qty));
        }
        public static void GetItemQuantity(List<Item> items)
        {
            Console.WriteLine(Environment.NewLine + "Total Quantities: "+ items.Sum(_ => _.Qty) + Environment.NewLine);
        }
        //Recurring Jobs
        //Display the names in List and Quantities

        [HttpGet("BackgroundJob-RecurringJobs")]
        public async Task<IActionResult> GetItems()
        {
            var items = await conn.Items.ToListAsync();
            //Run a job everyday at anytime (minute, hour, day of the month, month, day of the week)
            RecurringJob.AddOrUpdate("RecurringJobId", () => DoConsoleStuff(items), "* * * * *");
            //Run a job at 14:40 on the 20th of every month
            //RecurringJob.AddOrUpdate("RecurringJobId", () => DoConsoleStuff(items), "0 15 20 * *");
            return Ok(items);
        }
    }
}
