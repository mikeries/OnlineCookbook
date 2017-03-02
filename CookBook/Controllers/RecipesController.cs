using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using CookBook.Models;
using PagedList;

namespace CookBook.Controllers
{
    [Authorize]
    public class RecipesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

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
            string currentUser = User.Identity.GetUserId();

            var recipes = db.Recipes.Include(f => f.Ingredients).Where(u => u.UserID == currentUser);

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

            int pageSize = 5;
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

            string currentUser = User.Identity.GetUserId();

            Recipe recipe = db.Recipes.Where(u => u.UserID == currentUser).Where(x => x.RecipeID == id).Include(i => i.Ingredients).SingleOrDefault();
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
            recipe.Name = "New Recipe";
            recipe.UserID = User.Identity.GetUserId();
            db.Recipes.Add(recipe);
            db.SaveChanges();

            return RedirectToAction("Edit", new { id = recipe.RecipeID });
        }

        public ActionResult Copy(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string currentUser = User.Identity.GetUserId();
            Recipe recipe = db.Recipes.Where(u => u.UserID == currentUser)
                .Include("Ingredients").Include("Procedures").AsNoTracking().FirstOrDefault(e => id == e.RecipeID);

            if (recipe == null)
            {
                return HttpNotFound();
            }

            recipe.Name = recipe.Name + " (copy)";
            db.Recipes.Add(recipe);
            db.SaveChanges();
            return RedirectToAction("Edit", new { id = recipe.RecipeID });

        }

        // GET: Recipes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string currentUser = User.Identity.GetUserId();
            Recipe recipe = db.Recipes.Where(u=>u.UserID == currentUser).FirstOrDefault(r=>r.RecipeID == id);

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
        public ActionResult Edit([Bind(Include = "RecipeID,UserID,Name,Description,Servings,PreperationTime,InactiveTime,CookTime,Ingredients,Procedures")] Recipe recipe)
        {
            if (ModelState.IsValid)
            {
                Recipe dbRecipe = db.Recipes.Find(recipe.RecipeID);


                if (dbRecipe != null)
                {
                    string currentUser = User.Identity.GetUserId();
                    if (dbRecipe.UserID != currentUser)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                    }

                    removeDeletedIngredientsFromDB(recipe, dbRecipe);
                    updateRemainingIngredients(recipe);
                    removeDeletedProceduresFromDB(recipe, dbRecipe);
                    updateRemainingProcedures(recipe);

                    db.Entry(dbRecipe).CurrentValues.SetValues(recipe);
                    db.Entry(dbRecipe).State = EntityState.Modified;
                } else
                {
                    recipe.UserID = User.Identity.GetUserId();
                    db.Recipes.Add(recipe);
                }

                db.SaveChanges();

                return RedirectToAction("Edit", recipe.RecipeID);
            }
            return View(recipe);
        }

        // TODO: is there a way to make these next 4 functions generic and eliminate duplication?
        private void removeDeletedProceduresFromDB(Recipe recipe, Recipe dbRecipe)
        {
            List<Procedure> deletedProcedures = null;
            if (recipe.Procedures != null)
            {
                var deleted =
                    from i in dbRecipe.Procedures
                    where !recipe.Procedures.Any(x => x.ProcedureID == i.ProcedureID)
                    select i;
                deletedProcedures = deleted.ToList();
            }
            else
            {
                deletedProcedures = dbRecipe.Procedures;
            }

            foreach (Procedure i in deletedProcedures.ToList())
            {
                db.Procedures.Remove(i);
            }
        }

        private void updateRemainingProcedures(Recipe recipe)
        {
            if (recipe.Procedures != null)
            {
                var index = 0;
                foreach (Procedure i in recipe.Procedures)
                {
                    Procedure procedure;

                    if (i.ProcedureID == 0)
                    {
                        procedure = db.Procedures.Add(i);
                    }
                    else
                    {
                        procedure = db.Procedures.Find(i.ProcedureID);
                    }

                    db.Entry(procedure).CurrentValues.SetValues(i);
                    procedure.StepOrder = index++;
                    procedure.RecipeID = recipe.RecipeID;
                }
            }
        }

        private void removeDeletedIngredientsFromDB(Recipe recipe, Recipe dbRecipe)
        {

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
        }

        private void updateRemainingIngredients(Recipe recipe)
        {
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
        }

        // GET: Recipes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string currentUser = User.Identity.GetUserId();

            Recipe recipe = db.Recipes.Where(r=>r.UserID == currentUser).FirstOrDefault(r => r.RecipeID == id);
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
            if (recipe.UserID == User.Identity.GetUserId())
            {
                db.Recipes.Remove(recipe);
                db.SaveChanges();
            }
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
