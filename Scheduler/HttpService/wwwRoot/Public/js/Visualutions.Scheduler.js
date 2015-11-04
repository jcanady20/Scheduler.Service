window.app = (function ($, Backbone, _, undefined) {
	var m_self = {};
	var m_debug = true;
	var m_router = null;
	var m_apiUrl = "../api/";
	var m_currentView = null;
	var m_jobs = null;

	var _log = function (obj) {
		if (m_debug === true && window["console"] !== undefined) {
			console.log(obj);
		}
	}

	m_self.log = function (obj) {
		_log(obj);
	}

	var Models = {};
	var Collections = {};
	var Views = {};

	Models.Error = Backbone.Model.extend();
	Models.Job = Backbone.Model.extend({
		url: function () {
			return m_apiUrl + "jobs/" + this.get("id");
		}
	});
	Models.JobHistory = Backbone.Model.extend();
	Models.JobHistorySearch = Backbone.Model.extend({
		defaults: {
			page: 1,
			pageSize: 10
		}
	});
	Models.JobStep = Backbone.Model.extend({
		urlRoot: function () {
			return m_apiUrl + "jobs/steps";
		}
	});
	Models.JobSchedule = Backbone.Model.extend();
	Models.Schedule = Backbone.Model.extend();

	Collections.Jobs = Backbone.Collection.extend({
		model: Models.Job,
		url: function () {
			return m_apiUrl + "jobs";
		}
	});
	Collections.JobHistory = Backbone.Collection.extend({
		model: Models.JobHistory,
		totalPages: 0,
		totalCount: 0,
		url: function () {
			return m_apiUrl + "jobs/history";
		},
		parse: function (response, options) {
			this.totalPages = response.totalPages;
			this.totalCount = response.totalCount;
			return response.entities;

		}
	});
	Collections.JobSteps = Backbone.Collection.extend({
		model: Models.JobStep,
		jobId: null,
		initialize: function(options) {
			_.extend(this, _.pick(options, "jobId"));
		},
		url: function () {
			return m_apiUrl + "jobs/steps/" + this.jobId;
		}
	});
	Collections.JobSchedules = Backbone.Collection.extend();
	Collections.Schedules = Backbone.Collection.extend();

	Views.Empty = Backbone.View.extend({
		viewName: "Empty",
		tagName: "div",
		className: "",
		template: _.template($("#").html()),
		initialize: function () {
			app.log("Initializing " + this.viewName);
		},
		render: function () {
			this.$el.empty().append(this.template(this.model.toJSON()));
			return this;
		},
		events: {}
	});

	Views.Error = Backbone.View.extend({
		viewName: "Error",
		template: _.template($("#error-tmpl").html()),
		initialize: function () {
			_log("Initializing " + this.viewName);
		},
		render: function () {
			this.$el.empty();
			var jm = this.model.toJSON();
			this.$el.append(this.template(jm));
			return this;
		},
		showDialog: function () {
			this.$el.find(".modal").modal({
				backdrop: "static",
				keyboard: false,
				show: true
			});
		},
		OnClose: function (e) {
			e.preventDefault();
			this.remove();
		},
		events: {
			"click .btn-close": "OnClose"
		}
	});

	Views.Jobs = Backbone.View.extend({
		viewName: "Job",
		elName: "#main-content",
		childContainer: "#job-tbl-body",
		template: _.template($("#job-index-tmpl").html()),
		initialize: function () {
			_log("Initializing " + this.viewName);
			this.listenTo(m_jobs, 'change add remove update', this.renderChildren);
			this.listenTo(m_jobs, 'request', this.renderLoading);
			this.render();
		},
		renderLoading: function () {
			var ldr = _.template($("#job-loader-tmpl").html());
			this.childContainer.append(ldr());
		},
		removeChildren: function () {
			_.forEach(this.childViews, function (view) {
				view.remove();
			}, this);
		},
		remove: function () {
			this.removeChildren();
			Backbone.View.prototype.remove.call(this);
		},
		render: function () {
			this.$el.empty();
			this.$el.append(this.template());
			$(this.elName).append(this.$el);
			this.renderChildren();
			return this;
		},
		renderChildren: function () {
			var _slf = this;
			$(this.childContainer).empty();
			var tmpl = _.template($("#job-item-tmpl").html());
			m_jobs.forEach(function (item) {
				var m = item.toJSON();
				var r = tmpl(m);
				$(_slf.childContainer).append(r);
			}, this);
		}
	});

	Views.JobView = Backbone.View.extend({
		viewName: "JobView",
		elName: "#main-content",
		childView: null,
		childViewName: null,
		template: _.template($("#job-view-tmpl").html()),
		initialize: function (options) {
			_log("Initializing " + this.viewName);
			_.extend(this, _.pick(options, "childViewName"));
			this.render();
			this.renderChildView();
		},
		render: function () {
			_log("Rendering " + this.viewName);
			this.$el.empty();
			var m = this.model.toJSON();
			var t = this.template(m);
			this.$el.append(t);
			$(this.elName).append(this.el);
			return this;
		},
		renderChildView: function () {
			_log("Rendering childView for " + this.childViewName);
			switch (this.childViewName) {
				case "JobSteps":
					this.childView = new Views.JobSteps({ model: this.model });
					break;
				case "JobSchedules":
					this.childView = new Views.JobSchedules({ model: this.model });
					break;
				default:
					this.childView = new Views.JobEditor({ model: this.model });
					break;
			}
		}
	});

	Views.JobEditor = Backbone.View.extend({
		viewName: "JobEditor",
		elName: "#job-content",
		template: _.template($("#job-editor-tmpl").html()),
		initialize: function () {
			_log("Initializing " + this.viewName);
			this.render();
		},
		render: function () {
			_log("Rendering " + this.viewName);
			this.$el.empty();
			var m = this.model.toJSON();
			var t = this.template(m);
			this.$el.append(t);
			$(this.elName).append(this.el);
			return this;
		},
		events: { }
	});

	Views.JobSteps = Backbone.View.extend({
		viewName: "JobSteps",
		elName: "#job-content",
		childContainer: "#tbl-body",
		currrentSelected: null,
		currentEditor: null,
		template: _.template($("#job-steps-tmpl").html()),
		initialize: function () {
			_log("Initializing " + this.viewName);
			this.collection = new Collections.JobSteps({ jobId: this.model.get("id") });

			this.listenTo(this.collection, "request", this.renderLoading);
			this.listenTo(this.collection, "sync", this.renderChildren);

			this.collection.fetch({ data: { id: this.model.get("id") } });
			this.render();
		},
		render: function () {
			_log("Rendering " + this.viewName);
			this.$el.empty();
			var m = this.model.toJSON();
			var t = this.template(m);
			this.$el.append(t);
			$(this.elName).append(this.el);
			return this;
		},
		renderLoading: function () {
			$(this.childContainer).empty();
			var ldr = _.template($("#job-step-loader-tmpl").html());
			$(this.childContainer).append(ldr());
		},
		renderChildren: function () {
			$(this.childContainer).empty();
			var tmpl = _.template($("#job-step-detail-tmpl").html());
			if (this.collection.length === 0) {
				var t = _.template($("#job-step-noresults-tmpl").html());
				$(this.childContainer).append(t());
			} else {
				this.collection.forEach(function (item) {
					var m = item.toJSON();
					var r = tmpl(m);
					var x = $(r).data("model", m);
					$(this.childContainer).append(x);
				}, this);
				$(this.childContainer + ">tr:first").click();
			}
		},
		refresh: function () {
			_log("Refreshing Records");
			this.collection.fetch();
		},
		selectRow: function(e) {
			var tr = $(e.target).closest("tr")
			tr.siblings().removeClass("info");
			tr.addClass("info");
			var m = tr.data("model");
			this.currrentSelected = this.collection.get(m.id);
			$(".btn-moveup").prop("disabled", (m.stepId <= 1));
			$(".btn-movedown").prop("disabled", (m.stepId >= this.collection.length));
		},
		createStep: function() {
			var model = new Models.JobStep({ jobId: this.model.get("id"), stepId: this.collection.length });
			currentEditor = new Views.JobStepEditor({ model: m });
		},
		editStep: function() {
			if (this.currrentSelected === null) {
				_log("No currently selected record.");
			}
			new Views.JobStepEditor({ model: this.currrentSelected });
		},
		deleteStep: function() {
			if (this.currrentSelected === null) {
				_log("No currently selected record.");
			}

		},
		moveup: function() {
			if (this.currrentSelected === null) {
				return;
			}
			var model = this.currrentSelected;
			var mdls = this.collection;
			var index = mdls.indexOf(this.currrentSelected);
			if (index > 0) {
				mdls.remove(model, { silent: true });
				mdls.add(model, { at: index - 1 });
			}
		},
		movedown: function () {
			if (this.currrentSelected === null) {
				return;
			}
			var model = this.currrentSelected;
			var mdls = this.collection;
			var index = mdls.indexOf(this.currrentSelected);
			if (index < mdls.length) {
				mdls.remove(mdoel, { silen: true });
				mdls.add(model, { at: index + 1 });
			}
		},
		events: {
			"click .btn-refresh" : "refresh",
			"click tbody tr" : "selectRow",
			"click .btn-newStep" : "createStep",
			"click .btn-editStep" : "editStep",
			"click .btn-deleteStep" : "deleteStep",
			"click .btn-moveup" : "moveUp",
			"click btn-movedown" : "movedown"
		}
	});

	Views.JobStepEditor = Backbone.View.extend({
		viewName: "Error",
		template: _.template($("#jobstep-edit-tmpl").html()),
		initialize: function () {
			_log("Initializing " + this.viewName);
			this.render();
		},
		render: function () {
			this.$el.empty();
			var jm = this.model.toJSON();
			this.$el.append(this.template(jm));
			this.$el.appendTo("body");
			this.showDialog();
			return this;
		},
		showDialog: function () {
			this.$el.find(".modal").modal({
				backdrop: "static",
				keyboard: false,
				show: true
			});
		},
		OnClose: function (e) {
			e.preventDefault();
			this.remove();
		},
		OnSave: function(e) {
			e.preventDefault();

		},
		events: {
			"click .btn-cancel": "OnClose",
			"click .btn-save": "OnSave"
		}
	});

	Views.JobSchedules = Backbone.View.extend({
		viewName: "JobSchedules",
		elName: "#job-content",
		template: _.template($("#").html()),
		initialize: function () {
			_log("Initializing " + this.viewName);
		},
		render: function () {
			this.$el.empty();
			return this;
		},
		events: { }
	});

	Views.JobHistory = Backbone.View.extend({
		viewName: "JobHistory",
		elName: "#main-content",
		childContainer: "#tbl-body",
		pagerContainer: "#results-pager",
		template: _.template($("#job-history-tmpl").html()),
		initialize: function () {
			_log("Initializing " + this.viewName);
			this.collection = new Collections.JobHistory();

			this.listenTo(this.collection, "request", this.renderLoading);
			this.listenTo(this.collection, "sync", this.renderChildren);
			this.searchHistory();
			this.render();
		},
		renderLoading: function () {
			$(this.childContainer).empty();
			var ldr = _.template($("#job-history-loading-tmpl").html());
			$(this.childContainer).append(ldr());
		},
		render: function () {
			this.$el.empty();
			var m = this.model.toJSON();
			var t = this.template(m);
			this.$el.append(t);
			$(this.elName).append(this.el);
			return this;
		},
		renderChildren: function () {
			var _slf = this;
			$(this.childContainer).empty();
			var tmpl = _.template($("#job-history-detail-tmpl").html());
			_slf.collection.forEach(function (item) {
				var m = item.toJSON();
				var r = tmpl(m);
				$(_slf.childContainer).append(r);
			}, this);

			_slf.renderPager(_slf.collection.totalPages, _slf.model.get("page"));
		},
		searchHistory: function() {
			var _slf = this;
			var m = _slf.model.toJSON();
			_slf.collection.fetch({ data: m });
		},
		renderPager: function(pageCount, pageNumber) {
			var _slf = this;
			var container = $(_slf.pagerContainer);
			container.empty();

			if (pageCount <= 1) {
				return;
			}

			var pager = $("<ul>").addClass("pagination pagination-sm pull-right").css("margin", "4px, 0px !important");
			if (pageNumber === NaN) {
				pageNumber = 1;
			}
			var startPoint = 1;
			var endPoint = 9;
			if (pageNumber > 4) {
				startPoint = pageNumber - 1;
				endPoint = pageNumber + 4;
			}
			if (endPoint > pageCount) {
				startPoint = pageCount - 8;
				endPoint = pageCount;
			}
			if (startPoint < 1) {
				startPoint = 1;
			}

			pager.append(_slf.renderPagerButton('prev', pageNumber, pageNumber, pageCount));

			for (var page = startPoint; page <= endPoint; page++) {
				pager.append(_slf.renderPagerButton('', pageNumber, page, pageCount));
			}

			pager.append(_slf.renderPagerButton('next', pageNumber, pageNumber, pageCount));

			container.append(pager);
		},
		renderPagerButton: function (label, pageNumber, destPage, pageCount) {
			var id = this.model.get("jobId");
			var li = $("<li>");
			var btn = $("<a>").attr("href", "#/jobs/history/" + id + "/" + destPage);
			var ihtml = $("<span>");
			var c = "";
			switch (label) {
				case "prev":
					ihtml.addClass("fa fa-fw fa-chevron-left");
					if (pageNumber <= 1) {
						c = "hide";
					}
					break;
				case "next":
					ihtml.addClass("fa fa-fw fa-chevron-right");
					if (pageNumber >= pageCount) {
						c = "hide";
					}
					break;
				default:
					ihtml.text(destPage);
					if (pageNumber == destPage) {
						c = "active";
					}
					break;
			}

			btn.append(ihtml);
			return li.append(btn).addClass(c);
		},
		events: {
			"click .btn-refresh" : "searchHistory"
		}
	});

	Views.JobHistoryItem = Backbone.View.extend({
		viewName: "JobHistory",
		tagName: "tr",
		template: _.template($("#job-history-detail-tmpl").html()),
		initialize: function () {
			_log("Initializing " + this.viewName);
		},
		render: function () {
			this.$el.empty();
			var m = this.model.toJSON();
			this.$el.append(this.template(m));
			return this;
		}
	});

	var Router = Backbone.Router.extend({
		routes: {
			"": "defaultRoute",
			"jobs(/)": "defaultRoute",
			"jobs/:id": "viewJob",
			"jobs/start/:id" : "startJob",
			"jobs/delete/:id": "deleteJob",
			"jobs/history/:id(/:page)": "jobHistory",
			"jobs/steps/:id": "jobSteps",
			"jobs/schedules/:id": "jobSchedules",
			"steps/:id": "editjobStep",
			"steps/delete/:id": "deleteStep",
			"schedules": "schedules",
			"schedules/:id": "editSchedule",
			"schedules/delete/:id": "deleteSchedule"
		},
		defaultRoute: function () {
			_log("Default Route");
			if (m_currentView !== null) {
				m_currentView.remove();
			}
			m_currentView = new Views.Jobs();
		},
		viewJob: function (id) {
			_log("View Job Route " + id);
			if (m_currentView !== null) {
				m_currentView.remove();
			}
			var job = m_jobs.get(id);
			if (job === undefined) {
				job = new Models.Job({ id: id });
				var resp = job.fetch();
				resp.done(function () {
					m_currentView = new Views.JobView({ model: job, childViewName: "" });
				});
			} else {
				m_currentView = new Views.JobView({ model: job, childViewName : "" });
			}			
		},
		startJob: function () {
			_log("Start Job Route");

		},
		deleteJob: function (id) {
			_log("Delete Job Route");
		},
		jobHistory: function (id, page) {
			if (page === undefined || page === null || page === NaN) {
				page = 1;
			}
			_log("Job History Route id: [" + id + "] page: [" + page + "]");
			if (m_currentView !== null) {
				m_currentView.remove();
			}
			var historySearch = new Models.JobHistorySearch({ page: page });
			var job = m_jobs.get(id);
			if (job === undefined) {
				job = new Models.Job({ id: id });
				var resp = job.fetch();
				resp.done(function () {
					historySearch.set("jobId", job.get("id"));
					historySearch.set("name", job.get("name"));
					m_currentView = new Views.JobHistory({ model: historySearch });
				});
			} else {
				historySearch.set("jobId", job.get("id"));
				historySearch.set("name", job.get("name"));
				m_currentView = new Views.JobHistory({ model: historySearch });
			}
		},
		jobSteps: function (id) {
			_log("Job Steps Route");
			if (m_currentView !== null) {
				m_currentView.remove();
			}
			var job = m_jobs.get(id);
			if (job === undefined) {
				job = new Models.Job({ id: id });
				var resp = job.fetch();
				resp.done(function () {
					m_currentView = new Views.JobView({ model: job, childViewName: "JobSteps" });
				});
			} else {
				m_currentView = new Views.JobView({ model: job, childViewName: "JobSteps" });
			}
		},
		jobSchedules: function (id) {
			if (m_currentView !== null) {
				m_currentView.remove();
			}
			var job = m_jobs.get(id);
			if (job === undefined) {
				job = new Models.Job({ id: id });
				var resp = job.fetch();
				resp.done(function () {
					m_currentView = new Views.JobView({ model: job, childViewName: "JobSchedules" });
				});
			} else {
				m_currentView = new Views.JobView({ model: job, childViewName: "JobSchedules" });
			}
		},
		editjobStep: function (id, stepid) {
			_log("Edit Job Step Route");
		},
		deleteSteps: function (id) {
			_log("Delete Step Route");
		},
		schedules: function () {
			_log("Schedules Route");
		},
		editSchedule: function (id) {
			_log("Edit Schedule Route");
		},
		deleteSchedules: function (id) {
			_log("Delete Schedule Route");
		}
	});

	//	Self Executing Function
	//	Will be called in the script executes
	//	Magic of trailing ();
	initialize = (function () {
		_log("Initializing Scheduler Application");
		m_jobs = new Collections.Jobs();
		m_jobs.fetch();

		m_router = new Router;
		//	Start the Backbone history a necessary step for book-markable URL's
		Backbone.history.start();
	})();

	m_self.showError = (function (title, description, status, xhr) {
		var error = buildError(title, description, status, xhr);
		var mdl = new Models.Error(error);
		var errView = new Views.Error({ model: mdl });
		errView.render().$el.appendTo("body");
		errView.showDialog();
	});

	var buildError = (function (title, description, status, xhr) {
		var errorModel = { title: title, statusText: description, status: status, data: null };
		try {
			if (xhr != null) {
				var mr = JSON.parse(xhr.responseText);
				errorModel.data = mr;
			}
		} catch (err) {
			_log("Caught Error Parsing response");
			_log(err);
		}
		return errorModel;
	});

	return m_self;
})($, Backbone, _);


$(function () {

	//	Disable caching for ajax requests
	$.ajaxSetup({ cache: false });

	//	Global Handler for href=# links
	$(document).delegate('a[href$="#"]', "click", function (e) {
		e.preventDefault();
	});

	$(document).ajaxError(function (event, jqXHR, ajaxSettings, thrownError) {
		var title = "";
		var statusErrorMap = {
			400: "Server understood the request but request content was invalid",
			404: "Not Found",
			401: "Unauthorized access",
			403: "Forbidden resource can not be accessed",
			500: "Internal Server Error",
			503: "Service Unavailable"
		}
		if (jqXHR.status) {
			title = statusErrorMap[jqXHR.status];
			if (!title) {
				title = "Unknown Error";
			}
		} else if (event == "parsererror") {
			title = "Failed to Parse JSON Response";
		} else if (event == "timeout") {
			title = "Request Time out"
		} else if (event == 'abort') {
			title = "Request was aborted by the server";
		} else {
			title = "Unknown Error";
		}
		app.showError(title, jqXHR.statusText, jqXHR.status, jqXHR);
	});

	window.onerror = function (errorMsg, url, lineNumber, column, errorObj) {
		app.showError("Javascript Error", errorMsg, "Javascript Error", null);
		//	Return true to tell the browser you've handled the error yourself
		//	or Return false to let the browser run its error handler as well
		return false;
	}

});