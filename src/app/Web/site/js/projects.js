/* Models */
Project = Backbone.Model.extend({
    initialize: function() {
        if (!this.get("projecturl")) {
            this.set({"projecturl": this.EMPTY});
        }
    }
});

App.Projects = Backbone.Collection.extend({
   model: Project,
   url: 'projects'
});

window.Projects = new App.Projects;

/* Views */
App.Views.ProjectEdit = Backbone.View.extend({
    el: $('#content'),
    initialize: function() {
        _.bindAll(this, 'render');
        this.render();
    },
    events: {
        "click button": "addProject"
    },
    render: function() {
        ich.grabTemplates();
        this.el = ich.projectEdit(new Project());
        $('#content').html(this.el);
        return this;
    },
    addProject: function(event) {
        // this.model.save({projecturl: this.$('.projecturl').val()});
        Projects.create({projecturl: this.$('[name=projecturl]').val()},
            {
                success: function(model, response) {
                    console.info(model);
                    $(this.el).fadeOut("slow");
                },
                error: function() {
                    console.info('Error saving document!');
                }
                
        });
    }
    
});

/* Controllers */
App.Controllers.Projects = Backbone.Controller.extend({
   routes: {
       "":  "newProject",
       "new": "newProject"
   },
      
   newProject: function() {
       new App.Views.ProjectEdit({
           model: Project
           });
   }
});
