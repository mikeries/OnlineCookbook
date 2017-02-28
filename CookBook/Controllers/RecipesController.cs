using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CookBook.Models;
using PagedList;

namespace CookBook.Controllers
{
    public class RecipesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Recipes
        //public ActionResult Index()
        //{
        //    return View(db.Recipes.ToList());
        //}
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var recipes = db.Recipes.Include(f => f.Ingredients);

            if (!String.IsNullOrEmpty(searchString))
            {
                recipes = recipes.Where(s => s.Name.Contains(searchString)
                                       || s.Description.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    recipes = recipes.OrderByDescending(s => s.Name);
                    break;
                default:  // Name ascending 
                    recipes = recipes.OrderBy(s => s.Name);
                    break;
            }
            recipes = recipes.OrderBy(s => s.Name);

            int pageSize = 15;
            int pageNumber = (page ?? 1);
            return View(recipes.ToPagedList(pageNumber, pageSize));
        }

        // GET: Recipes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Recipe recipe = db.Recipes.Where(x => x.RecipeID == id).Include(i => i.Ingredients).SingleOrDefault();
            if (recipe == null)
            {
                return HttpNotFound();
            }
            return View(recipe);
        }

        // GET: Recipes/Create
        public ActionResult Create()
        {
            Recipe recipe = new Models.Recipe();
            return View(recipe);
        }

        // POST: Recipes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "RecipeID,Name,Description,Directions,Servings,PreperationTime,InactiveTime,CookTime,Comments")] Recipe recipe)
        {
            if (ModelState.IsValid)
            {
                db.Recipes.Add(recipe);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(recipe);
        }

        // GET: Recipes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Recipe recipe = db.Recipes.Find(id);
            if (recipe == null)
            {
                return HttpNotFound();
            }
            return View(recipe);
        }

        // POST: Recipes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "RecipeID,Name,Description,Directions,Servings,PreperationTime,InactiveTime,CookTime,Comments,Ingredients")] Recipe recipe)
        {
            if (ModelState.IsValid)
            {
                Recipe dbRecipe = db.Recipes.Find(recipe.RecipeID);

                List<Ingredient> deletedIngredients = null;
                if (recipe.Ingredients != null)
                {
                    var deleted =
                        from i in dbRecipe.Ingredients
                        where !recipe.Ingredients.Any(x => x.IngredientID == i.IngredientID)
                        select i;
                    deletedIngredients = deleted.ToList();
                }
                else
                {
                    deletedIngredients = dbRecipe.Ingredients;
                }

                foreach (Ingredient i in deletedIngredients.ToList())
                {
                    db.Ingredients.Remove(i);
                }

                if (recipe.Ingredients != null)
                {
                    var index = 0;
                    foreach (Ingredient i in recipe.Ingredients)
                    {
                        Ingredient ingredient;

                        if (i.IngredientID == 0)
                        {
                            ingredient = db.Ingredients.Add(i);
                        }
                        else
                        {
                            ingredient = db.Ingredients.Find(i.IngredientID);
                        }

                        db.Entry(ingredient).CurrentValues.SetValues(i);
                        ingredient.SortIndex = index++;
                        ingredient.RecipeID = recipe.RecipeID;
                    }
                }                

                db.Entry(dbRecipe).CurrentValues.SetValues(recipe);
                db.Entry(dbRecipe).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Edit", recipe.RecipeID);
            }
            return View(recipe);
        }

        // GET: Recipes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Recipe recipe = db.Recipes.Find(id);
            if (recipe == null)
            {
                return HttpNotFound();
            }
            return View(recipe);
        }

        // POST: Recipes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Recipe recipe = db.Recipes.Find(id);
            db.Recipes.Remove(recipe);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
