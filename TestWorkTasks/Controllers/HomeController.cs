using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Diagnostics;

namespace TestWorkTasks.Controllers
{
    public class leadsGroup
    {
        public int Id { get; set; }        
        public string Title { get; set; }
        public string Descript { get; set; }
        public string Executors { get; set; }
        public short Lead_status { get; set; }
        public DateTime Start_date { get; set; }
        public double Plan { get; set; }
        public double Lead_time { get; set; }
        public Nullable<DateTime> Completion_date { get; set; }
        public int TPid { get; set; }
        public int Ancestor { get; set; }
        public int Descendant { get; set; }
    }

    public class TreeResult
    {
        public string Tree { get; set; }
        public string TreeTabs { get; set; }
        public TreeResult()
        {
            this.Tree = "";
            this.TreeTabs = "";
        }
    }

    public class HomeController : Controller
    {
        string[] statusText = { "Назначена", "Выполняется", "Приостановлена", "Завершена"};

        //получение данных из leadsGroup
        public TreeResult getLeadResults(IOrderedQueryable<IGrouping<int, leadsGroup>> data, int rank)
        {
            TreeResult res = new TreeResult();
            foreach (var leadGroup in data)
            {
                if (leadGroup.Count() == rank)
                {
                    var lead = leadGroup.ToList().First();

                    res.Tree += "<li><a href='#lead"; res.Tree += lead.Id.ToString(); res.Tree += "'>";
                    var title = lead.Title == null ? ("Задача №" + lead.Id.ToString()) : lead.Title;
                    res.Tree += title;
                    res.Tree += "</a></li>";


                    res.TreeTabs += "<div id='lead"+ lead.Id.ToString() + "' class='tab'>";

                    res.TreeTabs += "<div class='title'><label>Заголовок:</label><h3><div class='field'>";
                    res.TreeTabs += title;
                    res.TreeTabs += "</div></hr></div>";

                    res.TreeTabs += "<div class='executors'><label>Исполнители:</label><div class='field'>";
                    res.TreeTabs += lead.Executors;
                    res.TreeTabs += "</div></div>";

                    res.TreeTabs += "<div class='date'><label>Дата создания задачи:</label><div class='field'>";
                    res.TreeTabs += lead.Start_date;
                    res.TreeTabs += "</div></div>";

                    res.TreeTabs += "<div class='status'><label>Статус:</label>";
                    res.TreeTabs += statusText[lead.Lead_status];
                    res.TreeTabs += "<div class='field hidden'>";
                    res.TreeTabs += lead.Lead_status;
                    res.TreeTabs += "</div></div>";

                    //Расчёт Плановая трудоёмкость задачи и Фактическое время выполнения.
                    //И получение списка подзадач
                    double plan = 0;
                    double lead_time = 0;
                    LinqWorkerDataContext myContext = new LinqWorkerDataContext(
                        "Data Source=localhost;Integrated Security=True"
                    );
                    var dataExecutPar = from leadsTree in myContext.LeadsTrees
                                        join leadsTreePath in myContext.LeadsTreePaths on
                                             leadsTree.id equals leadsTreePath.descendant
                                        join b in myContext.LeadsTreePaths on
                                             leadsTreePath.descendant equals b.descendant
                                        where leadsTreePath.ancestor == lead.Id
                                        group leadsTree
                                        by leadsTreePath.descendant into leadsTree
                                        orderby leadsTree.Key
                                        select leadsTree;
                    string subDiv = "<ul class='nav tree-func'>";
                    foreach (var execData in dataExecutPar)
                    {
                        if (execData.Count() >= rank)
                        {
                            var exData = execData.ToList().First();
                            plan += exData.plan.Equals(null) ? (Int16)0 : exData.plan.Value;
                            lead_time += exData.lead_time.Equals(null) ? (Int16)0 : exData.lead_time.Value;

                            if (execData.Count() != rank)
                            {
                                subDiv += "<li><a class='test' href='#lead"; subDiv += exData.id.ToString(); subDiv += "'>";
                                var leadTitle = exData.title == null ? ("Задача №" + exData.id.ToString()) : exData.title;
                                subDiv += leadTitle;
                                subDiv += "</a></li>";
                            }
                        }
                    }
                    subDiv += "</ul>";

                    res.TreeTabs += "<div class='plan'><label>Трудоёмкость:</label><div class='field'>";
                    res.TreeTabs += plan;
                    res.TreeTabs += "</div></div>";

                    res.TreeTabs += "<div class='lead_time'><label>Фактическое время выполнения:</label><div class='field'>";
                    res.TreeTabs += lead_time;
                    res.TreeTabs += "</div></div>";

                    res.TreeTabs += "<div class='completion_date'><label>Дата завершения:</label><div class='field'>";
                    res.TreeTabs += lead.Completion_date;
                    res.TreeTabs += "</div></div>";

                    res.TreeTabs += "<div class='descript'><label>Описание:</label><div class='field'>";
                    res.TreeTabs += lead.Descript;
                    res.TreeTabs += "</div></div>";

                    res.TreeTabs += "<div class='subDiv'><label>Подзадачи:</label><div class='field'>";
                    res.TreeTabs += subDiv;
                    res.TreeTabs += "</div></div>";

                    res.TreeTabs += "</div>";


                    //Получение подзадач
                    res.Tree += "<ul class='col-navTree'>";
                    var ancessorTree = getAncessorTree(lead, lead.Id, rank + 1);
                    res.Tree += ancessorTree.Tree;
                    res.Tree += "</ul>";
                    res.TreeTabs += ancessorTree.TreeTabs;
                }
            }
            return res;
        }

        public TreeResult getAncessorTree(leadsGroup tree, int id, int rank)
        {
            LinqWorkerDataContext myContext = new LinqWorkerDataContext(
                "Data Source=localhost;Integrated Security=True"
            );                       

            var data = from leadsTree in myContext.LeadsTrees
                       join leadsTreePath in myContext.LeadsTreePaths on
                            leadsTree.id equals leadsTreePath.descendant
                       join b in myContext.LeadsTreePaths on
                            leadsTreePath.descendant equals b.descendant
                       where leadsTreePath.ancestor == id
                       group new leadsGroup()
                       {
                           Id = leadsTree.id,
                           TPid = leadsTreePath.tpid,
                           Ancestor = leadsTreePath.ancestor,
                           Descendant = leadsTreePath.descendant,
                           Title = leadsTree.title,
                           Descript = leadsTree.descript == null ? "" : leadsTree.descript,
                           Executors = leadsTree.executors == null ? "" : leadsTree.executors,
                           Lead_status = leadsTree.lead_status.Equals(null) ? (Int16)0 : leadsTree.lead_status.Value,
                           Start_date = leadsTree.start_date.Equals(null) ? DateTime.Now : leadsTree.start_date.Value,
                           Plan = leadsTree.plan.Equals(null) ? (Int16)0 : leadsTree.plan.Value,
                           Lead_time = leadsTree.lead_time.Equals(null) ? (Int16)0 : leadsTree.lead_time.Value,
                           Completion_date = leadsTree.completion_date.Equals(null) ? (DateTime?)null : leadsTree.completion_date.Value
                       }
                       by leadsTreePath.descendant into leadsGroup
                       orderby leadsGroup.Key
                       select leadsGroup;            

            return getLeadResults(data, rank);
        }
        
        public TreeResult getTree()
        {
            LinqWorkerDataContext myContext = new LinqWorkerDataContext(
                "Data Source=localhost;Integrated Security=True"
            );

            var data = from leadsTree in myContext.LeadsTrees
                       join leadsTreePath in myContext.LeadsTreePaths on
                            leadsTree.id equals leadsTreePath.descendant
                       join b in myContext.LeadsTreePaths on
                            leadsTreePath.descendant equals b.descendant
                       group new leadsGroup()
                       {
                           Id = leadsTree.id,
                           TPid = leadsTreePath.tpid,
                           Ancestor = leadsTreePath.ancestor,
                           Descendant = leadsTreePath.descendant,
                           Title = leadsTree.title,
                           Descript = leadsTree.descript == null ? "" : leadsTree.descript,
                           Executors = leadsTree.executors == null ? "" : leadsTree.executors,
                           Lead_status = leadsTree.lead_status.Equals(null) ? (Int16)0 : leadsTree.lead_status.Value,
                           Start_date = leadsTree.start_date.Equals(null) ? DateTime.Now : leadsTree.start_date.Value,
                           Plan = leadsTree.plan.Equals(null) ? (Int16)0 : leadsTree.plan.Value,
                           Lead_time = leadsTree.lead_time.Equals(null) ? (Int16)0 : leadsTree.lead_time.Value,
                           Completion_date = leadsTree.completion_date.Equals(null) ? (DateTime?)null : leadsTree.completion_date.Value
                       }
                       by leadsTreePath.descendant into leadsGroup
                       orderby leadsGroup.Key
                       select leadsGroup;

            int rank = 1;
            TreeResult res = new TreeResult();
            res.Tree += "<ul class='col-navTree'>";
            var treeData = getLeadResults(data, rank);
            res.Tree += treeData.Tree;
            res.Tree += "</ul>";
            res.TreeTabs += treeData.TreeTabs;

            return res;
        }

        public ActionResult Index()
        {
            var res = getTree();
            ViewBag.Tree = res.Tree;
            ViewBag.LeadTabs = res.TreeTabs;

            return View();
        }
        
        //Создание новой задачи/подзадачи
        public ActionResult InsertLead()
        {
            LinqWorkerDataContext myContext = new LinqWorkerDataContext(
                "Data Source=localhost;Integrated Security=True"
            );       
            LeadsTree record = new LeadsTree();
            record.title = Request.Form["title"] == null ? "Задача #" : Request.Form["title"];
            record.descript = Request.Form["descript"] == null ? "Описание задачи" : Request.Form["descript"];
            record.executors = Request.Form["executors"] == null ? "Нет исполнителей" : Request.Form["executors"];
            record.lead_status = 0;
            record.plan = Request.Form["plan"] == null ? 0 : Convert.ToDouble(Request.Form["plan"]);
            record.lead_time = Request.Form["lead_time"] == null ? 0 : Convert.ToDouble(Request.Form["lead_time"]);
            record.start_date = DateTime.Now;
            myContext.LeadsTrees.InsertOnSubmit(record);
            myContext.SubmitChanges();
            int insertDescendant = (Request.Form["descendant"] == null) ? -1
                : Convert.ToInt32(Request.Form["descendant"]);//потомок вставки
            var data =  from leadsTreePath in myContext.LeadsTreePaths
                        where leadsTreePath.descendant == insertDescendant
                        select leadsTreePath.ancestor;
            foreach (var ancestor in data.ToList())
            {
                LeadsTreePath record2 = new LeadsTreePath();
                record2.ancestor = ancestor;
                record2.descendant = record.id;
                myContext.LeadsTreePaths.InsertOnSubmit(record2);
                myContext.SubmitChanges();
            }
            LeadsTreePath record3 = new LeadsTreePath();
            record3.ancestor = record.id;
            record3.descendant = record.id;
            myContext.LeadsTreePaths.InsertOnSubmit(record3);
            myContext.SubmitChanges();
            return Content("");
        }

        //Изменение задачи
        public ActionResult UpdateLead()
        {
            LinqWorkerDataContext myContext = new LinqWorkerDataContext(
                "Data Source=localhost;Integrated Security=True"
            );
            
            var id = Request.Form["id"] == null ? -1 : Int32.Parse(Request.Form["id"]);
            var title = Request.Form["title"] == null ? "Задача #" : Request.Form["title"];
            short status = Request.Form["status"] == null ? (Int16)0 : Int16.Parse(Request.Form["status"]);
            var executors = Request.Form["executors"] == null ? "" : Request.Form["executors"];
            var descript = Request.Form["descript"] == null ? "" : Request.Form["descript"];
            Nullable<DateTime> completion_date = null;
            if (Request.Form["completion_date"] != null && Request.Form["completion_date"] != "")
                completion_date = Convert.ToDateTime(Request.Form["completion_date"]);

            var data = from leadsTree in myContext.LeadsTrees
                       where leadsTree.id == id
                       select leadsTree;

            foreach (LeadsTree lead in data)
            {
                lead.title = title;

                if (status == 3)//Если статус "завершена"
                {
                    var data1 = from leadsTree in myContext.LeadsTrees
                                join leadsTreePath in myContext.LeadsTreePaths on
                                     leadsTree.id equals leadsTreePath.descendant
                                join b in myContext.LeadsTreePaths on
                                     leadsTreePath.descendant equals b.descendant
                                where leadsTreePath.ancestor == id
                                select leadsTree;
                    foreach (var subLead in data1)
                    {
                        if (subLead.lead_status == 0)
                        {
                            Response.StatusCode = 501;
                            Response.StatusDescription = "Status fail";
                            Response.ContentType = "text/plain";
                            Response.Write("Статус задачи или одной из её подзадач не может быть изменён. (Одна или несколько задач имеют статус 'Назначена')");
                            return Content("");
                        }
                        else
                        {
                            subLead.lead_status = status;
                            subLead.completion_date = DateTime.Now;
                        }
                    }
                    if (lead.lead_status == 0)
                    {
                        Response.StatusCode = 501;
                        Response.StatusDescription = "Status fail";
                        Response.ContentType = "text/plain";
                        Response.Write("Статус задачи или одной из её подзадач не может быть изменён. (Одна или несколько задач имеют статус 'Назначена')");
                        return Content("");
                    }
                    else
                    {
                        lead.lead_status = status;
                        lead.completion_date = DateTime.Now;
                    }
                }
                else
                {
                    lead.lead_status = status;
                }
                
                lead.executors = executors;
                lead.descript = descript;
            }
            myContext.SubmitChanges();
            return Content("");
        }

        //удаление задач
        public ActionResult DeleteLead()
        {
            LinqWorkerDataContext myContext = new LinqWorkerDataContext(
                "Data Source=localhost;Integrated Security=True"
            );
            var id = Request.Form["id"] == null ? -1 : Int32.Parse(Request.Form["id"]);

            //Удаление предков
            var data1 = from leadsTreePath in myContext.LeadsTreePaths
                       where leadsTreePath.ancestor == id
                       select leadsTreePath;
            foreach (var lead in data1)
            {
                myContext.LeadsTreePaths.DeleteOnSubmit(lead);
            }
            //Удаление потомков
            var data2 = from leadsTreePath in myContext.LeadsTreePaths
                        where leadsTreePath.descendant == id
                        select leadsTreePath;
            foreach (var lead in data2)
            {
                myContext.LeadsTreePaths.DeleteOnSubmit(lead);
            }
            //удаление задачи
            var data3 = from leadsTree in myContext.LeadsTrees
                       where leadsTree.id == id
                       select leadsTree;
            foreach (var lead in data3)
            {
                myContext.LeadsTrees.DeleteOnSubmit(lead);
            }
            myContext.SubmitChanges();
            return Content("");
        }
        //public ActionResult About()
        //{
        //    ViewBag.Message = "Your application description page.";

        //    return View();
        //}

        //public ActionResult Contact()
        //{
        //    ViewBag.Message = "Your contact page.";

        //    return View();
        //}
    }
}