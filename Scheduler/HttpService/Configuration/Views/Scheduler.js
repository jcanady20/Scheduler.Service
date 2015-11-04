﻿/*jshint bitwise: false, strict: false, browser: true, jquery: true, -W069*/
/*global $, jQuery, alert, ko, console, moment, Backbone, _*/
//    Returns the A/P Meridian indication for the Time
//    AM/PM
Date.prototype.getMeridian = function () {
    "use strict";
    var h = this.getHours();
    return (h >= 12 ? "PM" : "AM");
};

//    Returns a 12 hour time with the meridian
//    1:05 PM
Date.prototype.getOClockTime = function () {
    "use strict";
    var h = this.getHours();
    var m = String("00" + this.getMinutes()).slice(-2);
    var timeString = "";
    if (h > 12) {
        h = h - 12;
    }
    if (h === 0) {
        h = 12;
    }
    timeString += String("00" + h).slice(-2);
    timeString += ":" + m + " " + this.getMeridian();
    return timeString;
};

//    Returns Date String
//    01/01/1900 - 12/31/2099
Date.prototype.getDateString = function () {
    "use strict";
    var m = this.getMonth();
    var d = this.getDate();
    var y = this.getFullYear();
    return String("00" + (m + 1)).slice(-2) + "/" + String("00" + d).slice(-2) + "/" + y;
};

ko.bindingHandlers.dateInput = {
    init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        "use strict";
        var $el = $(element);
        var property = valueAccessor();
        var options = {
            keyboardNavigation: false,
            startDate: '01/01/1980',
            endDate: '12/31/2099',
            autoclose: true,
            todayBtn: 'linked',
            todayHighlight: true
        };
        $el.datepicker(options);
        ko.utils.registerEventHandler(element, "changeDate.bs.datepicker", function (e) {
            var d = moment(e.date).format("L");
            property(d);
        });
        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
            $el.datepicker("remove");
            $el = null;
        });
    },
    update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        "use strict";
        var $el = $(element);
        var val = ko.unwrap(valueAccessor());
        var dpval = moment($el.datepicker('getDate')).format("L");
        if (val !== dpval) {
            $el.val(val);
            $el.datepicker('update');
        }
    }
};
ko.bindingHandlers.timeInput = {
    init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        "use strict";
        var $el = $(element);
        var prop = valueAccessor();
        ko.utils.registerEventHandler(element, "blur", function (e) {
            var iptgrp = $el.closest(".input-group");
            if (iptgrp) {
                iptgrp.removeClass("has-error");
            }
            //    validate the input
            var val = $el.val();
            var t = new Date("01/01/1990 " + val);
            if (t === null || t === undefined || t === "Invalid Date") {
                $el.val(prop());
                $el.focus();
                if (iptgrp) {
                    iptgrp.addClass("has-error");
                }
            } else {
                prop(val);
            }
        });
    },
    update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        "use strict";
        var $el = $(element);
        var val = ko.unwrap(valueAccessor());
        $el.val(val);
    }
};
ko.bindingHandlers.numericOnly = {
    init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        "use strict";
        var $el = $(element);
        var prop = valueAccessor();
        ko.utils.registerEventHandler(element, "keydown", function (e) {
            var ekc = e.keyCode;
            // Allow: backspace, delete, tab, escape, and enter
            if (ekc === 46 || ekc === 8 || ekc === 9 || ekc === 27 || ekc === 13 ||
                // Allow: Ctrl+A
                    (ekc === 65 && e.ctrlKey === true) ||
                // Allow: . ,
                    (ekc === 188 || ekc === 190 || ekc === 110) ||
                // Allow: home, end, left, right
                    (ekc >= 35 && ekc <= 39)) {
                // let it happen, don't do anything
                return;
            } else {
                // Ensure that it is a number and stop the keypress
                if (e.shiftKey || (ekc < 48 || ekc > 57) && (ekc < 96 || ekc > 105)) {
                    e.preventDefault();
                } else {
                    prop($el.val());
                }
            }
        });
    },
    update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        "use strict";
        var $el = $(element);
        var val = ko.unwrap(valueAccessor());
        var x = parseInt(val, 10);
        if (x) {
            $el.val(x);
        }
    }
};

//    Standard enums for different actions and options for scheduling
window.enums = {
    FrequencyType: { OneTimeOnly: 1, Daily: 4, Weekly: 8, Monthly: 16, MonthlyRelative: 32, OnStartup: 64 },
    JobCompleteionAction: { Never: 0, OnSuccess: 1, OnFailure: 2, Always: 3 },
    JobStatus: { Executing: 1, WaitingForWorkerThread: 2, BetweenRetries: 3, Idle: 4, Suspended: 5, WaitingForStepToFinish: 6, PerformingCompletionAction: 7 },
    JobStepOutCome: { Failed: 0, Succeeded: 1, Retry: 2, Canceled: 3, Unknown: 5 },
    MonthlyInterval: { NotUsed: 0, Sunday: 1, Monday: 2, Tuesday: 4, Wednesday: 8, Thursday: 16, Friday: 32, Saturday: 64, Day: 127, WeekDay: 62, WeekendDay: 65 },
    NextStepAction: { NextStep: 1, QuitSuccess: 2, QuitFailure: 3 },
    RelativeInterval: { NotUsed: 0, First: 1, Second: 2, Third: 4, Fourth: 8, Last: 16 },
    RunRequestSource: { Scheduler: 1, Boot: 3, User: 4, OnIdle: 6 },
    SubIntervalType: { SpecificTime: 1, Minutes: 4, Hours: 8 },
    WeeklyInterval: { Sunday: 1, Monday: 2, Tuesday: 4, Wednesday: 8, Thursday: 16, Friday: 32, Saturday: 64 }
};

window.app = (function (window, $, ko, _, Backbone) {
    "use strict";
    var m_self = {};
    var m_debug = true;
    var m_router = null;
    var m_apiUrl = "http://localhost:8180/api/";
    var m_currentView = null;
    var m_jobActivityCollection = null;
    var m_subSystems = null;

    var _log = function (obj) {
        if (m_debug === true && window.console !== undefined) {
            console.log(obj);
        }
    };
    m_self.log = function (obj) {
        _log(obj);
    };

    var Models = {};
    var Collections = {};
    var Views = {};
    var ViewModels = {};

    Models.Error = Backbone.Model.extend();
    Models.Job = Backbone.Model.extend({
        defaults: {
            name: "New Job",
            enabled: true
        },
        url: function () {
            if (this.isNew()) {
                return m_apiUrl + "jobs";
            } else {
                return m_apiUrl + "jobs/" + this.get("id");
            }
        }
    });
    Models.JobActivity = Backbone.Model.extend();
    Models.JobHistory = Backbone.Model.extend();
    Models.JobHistorySearch = Backbone.Model.extend({
        defaults: {
            page: 1,
            pageSize: 10
        }
    });
    Models.JobStep = Backbone.Model.extend({
        defaults: {
            name: "New Step",
            subSystem: "WebTask",
            command: ""
        },
        urlRoot: function () {
            return m_apiUrl + "steps";
        }
    });
    Models.JobSchedule = Backbone.Model.extend({
        urlRoot: function () {
            return m_apiUrl + "jobschedules";
        }
    });

    //    Collections
    Collections.Jobs = Backbone.Collection.extend({
        model: Models.Job,
        url: function () {
            return m_apiUrl + "jobs";
        }
    });
    Collections.JobActivity = Backbone.Collection.extend({
        model: Models.JobActivity,
        url: function () {
            return m_apiUrl + "jobs/activity";
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
        comparator: 'stepId',
        initialize: function (options) {
            _.extend(this, _.pick(options, "jobId"));
        },
        url: function () {
            return m_apiUrl + "jobs/steps/" + this.jobId;
        }
    });
    Collections.JobSchedules = Backbone.Collection.extend({
        model: Models.JobSchedule,
        jobId: null,
        initialize: function (options) {
            _.extend(this, _.pick(options, "jobId"));
        },
        url: function () {
            if (this.jobId !== null) {
                return m_apiUrl + "jobs/schedules/" + this.jobId;
            }
        }
    });
    Collections.SubSystems = Backbone.Collection.extend({
        url: function () {
            return m_apiUrl + "jobs/subsystems";
        }
    });


    ViewModels.Base = function () {
        var _slf = this;
        _slf.setData = function (data) {
            var key;
            if (data === null || data === undefined) {
                return;
            }
            for (key in data) {
                if (data.hasOwnProperty(key)) {
                    if (ko.isObservable(this[key])) {
                        this[key](data[key]);
                    } else {
                        this[key] = data[key];
                    }
                }
            }
        };
        _slf.toJS = function () {
            var key;
            var x = {};
            for (key in this) {
                if (this.hasOwnProperty(key)) {
                    if (ko.isObservable(this[key])) {
                        x[key] = this[key]();
                    } else {
                        x[key] = this[key];
                    }
                }
            }
            return x;
        };
        return _slf;
    };
    ViewModels.CreateJob = function () {
        var _slf = this;
        _.extend(_slf, new ViewModels.Base());

        _slf.name = ko.observable();
        _slf.description = ko.observable();

        return _slf;
    };
    ViewModels.DeleteJob = function () {
        var _slf = this;
        _.extend(_slf, new ViewModels.CreateJob());
        return _slf;
    };
    ViewModels.JobEditor = function () {
        var _slf = this;
        _.extend(_slf, new ViewModels.Base());
        _slf.id = ko.observable();
        _slf.name = ko.observable();
        _slf.enabled = ko.observable(true);
        _slf.description = ko.observable();
        _slf.startStep = ko.observable();
        return _slf;
    };
    ViewModels.JobStep = function () {
        var _slf = this;
        _.extend(_slf, new ViewModels.Base());
        _slf.subSystems = m_subSystems.toJSON();
        _slf.id = ko.observable();
        _slf.jobId = ko.observable();
        _slf.enabled = ko.observable(true);
        _slf.name = ko.observable();
        _slf.subSystem = ko.observable();
        _slf.command = ko.observable();
        _slf.isVisShipped = ko.observable(false);
        _slf.databaseName = ko.observable();
        _slf.userName = ko.observable();
        _slf.password = ko.observable();

        return _slf;
    };
    ViewModels.JobStepConfirmDelete = function () {
        var _slf = this;
        _.extend(_slf, new ViewModels.Base());
        _slf.name = ko.observable();
        _slf.stepId = ko.observable();
        _slf.subSystem = ko.observable();
        return _slf;
    };
    ViewModels.JobScheduleConfirmDelete = function () {
        var _slf = this;
        _.extend(_slf, new ViewModels.Base());
        _slf.name = ko.observable();
        _slf.stepId = ko.observable();
        _slf.subSystem = ko.observable();
        return _slf;
    };
    ViewModels.Schedule = function (data) {
        var _slf = this;
        _slf.name = ko.observable("New Schedule");
        _slf.enabled = ko.observable(true);
        _slf.type = ko.observable(window.enums.FrequencyType.Daily);
        _slf.interval = ko.observable(1);
        _slf.subdayType = ko.observable(window.enums.SubIntervalType.Hours);
        _slf.subdayInterval = ko.observable(1);
        _slf.relativeInterval = ko.observable(0);
        _slf.recurrenceFactor = ko.observable(1);
        _slf.weeklyIntervals = ko.observableArray();
        _slf.subDayIntervalLabel = ko.pureComputed(function () {
            var x = parseInt(_slf.subdayType(), 10);
            if (x === window.enums.SubIntervalType.SpecificTime) {
                return "Time";
            } else if (x === window.enums.SubIntervalType.Hours) {
                return "Hour(s)";
            } else if (x === window.enums.SubIntervalType.Minutes) {
                return "Minute(s)";
            }
        }, _slf);
        _slf.startDate = ko.observable();
        _slf.startTime = ko.observable();
        _slf.endDate = ko.observable();
        _slf.endTime = ko.observable();
        var getDescription = function () {
            var xType = parseInt(_slf.type(), 10);
            var msg = "";
            msg += "Occurs";

            if (xType === window.enums.FrequencyType.OneTimeOnly) {
                msg += " one time only at " + _slf.startTime() + " on " + _slf.startDate();
                return msg;
            } else if (xType === window.enums.FrequencyType.Daily) {
                msg += " every ";
                if (_slf.interval() > 1) {
                    msg += _slf.interval();
                }
                msg += " day";
                if (_slf.interval() > 1) {
                    msg += "(s)";
                }
            } else if (xType === window.enums.FrequencyType.Weekly) {
                msg += " " + _slf.recurrenceFactor() + " week(s)";
                msg += " on ";
                //    Get WeekDays
                msg += _.keys(_.pick(window.enums.WeeklyInterval, function (x) { return x & _slf.interval(); }, this)).join(",");
            } else if (xType === window.enums.FrequencyType.Monthly) {
                msg += " every month(s) on day " + _slf.interval() + ".";
            } else if (xType === window.enums.FrequencyType.MonthlyRelative) {
                msg += " on the " + _.invert(window.enums.RelativeInterval)[_slf.relativeInterval()] + " " + _.invert(window.enums.MonthlyInterval)[_slf.interval()] + " of every " + _slf.recurrenceFactor() + " month(s).";
            } else if (xType === window.enums.FrequencyType.OnStartup) {
                msg += " when the schedule agent starts.";
                return msg;
            }

            //    Sub-day Details
            switch (parseInt(_slf.subdayType(), 10)) {
                case window.enums.SubIntervalType.SpecificTime:    //    At Specific Time
                    msg += " at " + _slf.startTime() + ".";
                    break;
                case window.enums.SubIntervalType.Minutes:    //    Minutes
                    msg += " every " + _slf.subdayInterval() + " minutes(s) between " + _slf.startTime() + " and " + _slf.endTime() + ".";
                    break;
                case window.enums.SubIntervalType.Hours:    //    Hours
                    msg += " every " + _slf.subdayInterval() + " hours(s) between " + _slf.startTime() + " and " + _slf.endTime() + ".";
                    break;
            }

            msg += " Schedule will be used starting on " + _slf.startDate();
            return msg;
        };
        _slf.description = ko.pureComputed(getDescription);
        _slf.setData = function (data) {
            if (data) {
                if (data.name) {
                    _slf.name(data.name);
                }
                if (data.enabled !== undefined) {
                    _slf.enabled(data.enabled);
                }
                if (data.type) {
                    _slf.type(data.type);
                }
                if (data.interval) {
                    _slf.interval(data.interval);
                }
                if (data.subdayType) {
                    _slf.subdayType(data.subdayType);
                }
                if (data.subdayInterval) {
                    _slf.subdayInterval(data.subdayInterval);
                }
                if (data.relativeInterval) {
                    _slf.relativeInterval(data.relativeInterval);
                }
                if (data.recurrenceFactor) {
                    _slf.recurrenceFactor(data.recurrenceFactor);
                }
                if (data.startDateTime) {
                    var startDateTime = new Date(data.startDateTime);
                    _slf.startDate(startDateTime.getDateString());
                    _slf.startTime(startDateTime.getOClockTime());
                }
                if (data.endDateTime) {
                    var endDateTime = new Date(data.endDateTime);
                    _slf.endDate(endDateTime.getDateString());
                    _slf.endTime(endDateTime.getOClockTime());
                }
            }
        };

        var init = (function (data) {
            _log("Initializing Schedule");
            var _newStartDate = new Date();
            var _newEndDate = new Date("12/31/2099 11:59 PM");
            _slf.startDate(_newStartDate.getDateString());
            _slf.startTime(_newStartDate.getOClockTime());

            _slf.endDate(_newEndDate.getDateString());
            _slf.endTime(_newEndDate.getOClockTime());

            var SetWeekDaySelections = function () {
                _log("Setting up Weekday Selections");
                var interval = parseInt(_slf.interval(), 10);
                var initialValues = _.values(_.pick(window.enums.WeeklyInterval, function (x) { return x & interval; }, this));
                ko.utils.arrayPushAll(_slf.weeklyIntervals, initialValues);
            };

            _slf.weeklyIntervals.subscribe(function (newValue) {
                var y = 0;
                _.forEach(newValue, function (x) { y = y | x; });
                _slf.interval(y);
            }, this);

            _slf.type.subscribe(function (newValue) {
                var x = parseInt(newValue, 10);
                if (x !== undefined && isNaN(x) === false && x === window.enums.FrequencyType.OneTimeOnly) {
                    _slf.subdayType(window.enums.SubIntervalType.SpecificTime);
                }
            }, this);

            _slf.subdayType.subscribe(function (newValue) {
                if (newValue === "") {
                    _slf.subdayType(8);
                }
            }, this);
            _slf.setData(data);
        }(data));
        _slf.toJS = function () {
            //    Remove Functions from object
            var removeFunctions = function (obj) {
                var i;
                for (i in obj) {
                    if (obj.hasOwnProperty(i) && obj[i] instanceof Function) {
                        delete obj[i];
                    }
                    if (obj.hasOwnProperty(i) && obj[i] instanceof Object) {
                        removeFunctions(obj[i]);
                    }
                }
            };
            //    create a copy of the object
            var copy = ko.toJS(_slf);
            //    remove functions
            removeFunctions(copy);
            //    Recombine the startDate + startTime
            copy.startDateTime = new Date(copy.startDate + " " + copy.startTime);
            //    Recombine the endDate + endTime
            copy.endDateTime = new Date(copy.endDate + " " + copy.endTime);
            //    Remove unused properties
            delete copy["startDate"];
            delete copy["startTime"];
            delete copy["endDate"];
            delete copy["endTime"];
            delete copy["description"];
            delete copy["weeklyIntervals"];
            delete copy["subDayIntervalLabel"];
            //    return the js object to the caller
            return copy;
        };
        _slf.saveClick = function () {
            console.log(_slf.toJS());
        };
        return _slf;
    };

    //    Views
    Views.Empty = Backbone.View.extend({
        viewName: "Empty",
        tagName: "div",
        className: "",
        template: _.template($("#").html()),
        initialize: function () {
            window.app.log("Initializing " + this.viewName);
        },
        render: function () {
            this.$el.empty().append(this.template(this.model.toJSON()));
            return this;
        },
        events: {}
    });

    Views.Error = Backbone.View.extend({
        viewName: "ErrorView",
        className: "modal fade",
        template: _.template($("#error-tmpl").html()),
        initialize: function () {
            _log("Initializing " + this.viewName);
        },
        render: function () {
            this.$el.empty();
            if (this.model !== null) {
                var jm = this.model.toJSON();
                this.$el.append(this.template(jm));
            }
            return this;
        },
        showDialog: function () {
            this.$el.modal({
                backdrop: false,
                keyboard: false,
                show: true
            });
        },
        OnClose: function (e) {
            e.preventDefault();
            this.$el.modal('hide');
        },
        remove: function () {
            Backbone.View.prototype.remove.call(this);
        },
        events: {
            "hidden.bs.modal": "remove",
            "click .btn-close": "OnClose"
        }
    });

    Views.JobActivity = Backbone.View.extend({
        viewName: "JobActivity",
        elName: "#main-content",
        childContainer: "#tbl-body",
        columnCount: 10,
        template: _.template($("#jobactivity-tmpl").html()),
        initialize: function () {
            _log("Initializing " + this.viewName);
            this.collection = m_jobActivityCollection;
            this.listenTo(this.collection, 'add remove update sync', this.renderChildren);
            this.listenTo(this.collection, 'request', this.renderLoading);
            this.collection.fetch();
            this.render();
        },
        render: function () {
            this.$el.empty();
            this.$el.append(this.template());
            $(this.elName).append(this.$el);
            return this;
        },
        renderLoading: function () {
            this.removeChildren();
            var ldr = _.template($("#loader-tmpl").html());
            $(this.childContainer).append(ldr({ colspan: this.columnCount }));
        },
        renderChildren: function () {
            $(this.childContainer).empty();
            var tmpl = _.template($("#jobactivity-item-tmpl").html());
            this.collection.forEach(function (item) {
                var m = item.toJSON();
                var r = tmpl(m);
                var x = $(r).data("model", m);
                $(this.childContainer).append(x);
            }, this);
        },
        removeChildren: function () {
            $(this.childContainer).empty();
        },
        remove: function () {
            this.removeChildren();
            Backbone.View.prototype.remove.call(this);
        },
        onRefreshClick: function () {
            this.collection.fetch();
        },
        onCreateJobClick: function () {
            new Views.JobCreate().showDialog();
        },
        onDeleteJobClick: function (e) {
            var tr = $(e.currentTarget).closest("tr");
            var d = tr.data("model");
            var m = m_jobActivityCollection.get(d.id);
            new Views.ConfirmJobDelete({ model: m }).showDialog();
        },
        onStartJobClick: function (e) {
            var tr = $(e.currentTarget).closest("tr");
            var m = tr.data("model");
            var url = m_apiUrl + "jobs/start/" + m.id;
            $.getJSON(url);
        },
        events: {
            "click .btn-refresh": "onRefreshClick",
            "click .btn-createJob": "onCreateJobClick",
            "click .btn-deleteJob": "onDeleteJobClick",
            "click .btn-startJob": "onStartJobClick"
        }
    });

    Views.JobCreate = Backbone.View.extend({
        viewName: "JobCreate",
        className: "modal fade",
        template: _.template($("#job-create-tmpl").html()),
        koViewModel: null,
        initialize: function (options) {
            _log("Initializing " + this.viewName);
            this.koViewModel = new ViewModels.CreateJob();
            this.model = new Models.Job();
            this.render();
        },
        render: function () {
            this.$el.empty();
            var jm = this.model.toJSON();
            this.koViewModel.setData(jm);

            this.$el.append(this.template(jm));
            this.$el.appendTo("body");
            this.showDialog();
            //    Apply Knockout Binding to the Element
            ko.applyBindings(this.koViewModel, this.el);
            return this;
        },
        remove: function () {
            ko.cleanNode(this.el);
            Backbone.View.prototype.remove.call(this);
        },
        showDialog: function () {
            this.$el.modal({ backdrop: "static", keyboard: false, show: true });
        },
        OnClose: function () {
            this.$el.modal('hide');
        },
        OnSave: function () {
            var _slf = this;
            _log("Saving new Job");
            $(".btn-save").button("loading");
            var km = this.koViewModel.toJS();
            this.model.set(km, { silent: true });
            var resp = this.model.save();
            resp.done(function () {
                //    Add the model to the jobsCollection
                m_jobActivityCollection.fetch();
                _slf.OnClose();
            });
            resp.always(function () {
                $(".btn-save").button("reset");
            });
        },
        events: {
            "hidden.bs.modal": "remove",
            "click .btn-cancel": "OnClose",
            "click .btn-save": "OnSave"
        }
    });

    Views.ConfirmJobDelete = Backbone.View.extend({
        viewName: "JobDelete",
        className: "modal fade",
        template: _.template($("#job-confirm-delete-tmpl").html()),
        koViewModel: null,
        initialize: function (options) {
            _log("Initializing " + this.viewName);
            this.koViewModel = new ViewModels.DeleteJob();
            this.render();
        },
        render: function () {
            this.$el.empty();
            var jm = this.model.toJSON();
            this.koViewModel.setData(jm);

            this.$el.append(this.template(jm));
            this.$el.appendTo("body");
            this.showDialog();
            //    Apply Knockout Binding to the Element
            ko.applyBindings(this.koViewModel, this.el);
            return this;
        },
        remove: function () {
            ko.cleanNode(this.el);
            Backbone.View.prototype.remove.call(this);
        },
        showDialog: function () {
            this.$el.modal({ backdrop: "static", keyboard: false, show: true });
        },
        onAcceptClick: function () {
            var _slf = this;
            //    Handle the Deletion
            var resp = this.model.destroy({ wait: true });
            resp.done(function () {
                _slf.OnClose();
            });
        },
        OnClose: function () {
            this.$el.modal('hide');
        },
        events: {
            "hidden.bs.modal": "remove",
            "click .btn-cancel": "OnClose",
            "click .btn-accept": "onAcceptClick"
        }
    });

    Views.JobEdit = Backbone.View.extend({
        viewName: "JobView",
        elName: "#main-content",
        childView: null,
        childViewName: null,
        template: _.template($("#job-edit-tmpl").html()),
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
        remove: function () {
            if (this.childView !== null) {
                this.childView.remove();
            }
            Backbone.View.prototype.remove.call(this);
        },
        renderChildView: function () {
            _log("Rendering childView for " + this.childViewName);
            switch (this.childViewName) {
                case "JobSteps":
                    this.childView = new Views.JobSteps({ model: this.model });
                    $(".list-group > .lstgrp-steps").addClass("active").siblings().removeClass("active");
                    break;
                case "JobSchedules":
                    this.childView = new Views.JobSchedules({ model: this.model });
                    $(".list-group > .lstgrp-schedules").addClass("active").siblings().removeClass("active");
                    break;
                default:
                    this.childView = new Views.JobEditor({ model: this.model });
                    $(".list-group > .lstgrp-general").addClass("active").siblings().removeClass("active");
                    break;
            }
        }
    });

    Views.JobEditor = Backbone.View.extend({
        viewName: "JobEditor",
        elName: "#job-content",
        koViewModel: null,
        template: _.template($("#job-editor-tmpl").html()),
        initialize: function () {
            _log("Initializing " + this.viewName);
            this.koViewModel = new ViewModels.JobEditor();
            this.render();
        },
        remove: function () {
            //    remove Knockout Binding
            ko.cleanNode(this.el);
            Backbone.View.prototype.remove.call(this);
        },
        render: function () {
            _log("Rendering " + this.viewName);
            this.$el.empty();
            var m = this.model.toJSON();
            this.koViewModel.setData(m);
            var tmpl = this.template(m);
            this.$el.append(tmpl);
            $(this.elName).append(this.el);

            ko.applyBindings(this.koViewModel, this.el);
            return this;
        },
        onSaveClick: function () {
            $(".btn-save").button("loading");
            var m = this.koViewModel.toJS();
            this.model.set(m, { silent: true });
            var resp = this.model.save();
            resp.done(function () {
                m_router.navigate("#/jobs", { trigger: true });
            });
            resp.always(function () {
                $(".btn-save").button("reset");
            });
        },
        events: {
            "click .btn-save": "onSaveClick"
        }
    });

    Views.JobSteps = Backbone.View.extend({
        viewName: "JobSteps",
        elName: "#job-content",
        childContainer: "#tbl-body",
        currrentSelected: null,
        columnCount: 3,
        template: _.template($("#job-steps-tmpl").html()),
        initialize: function () {
            _log("Initializing " + this.viewName);
            this.collection = new Collections.JobSteps({ jobId: this.model.get("id") });

            this.listenTo(this.collection, "request", this.renderLoading);
            this.listenTo(this.collection, "add remove update sync", this.renderChildren);

            this.collection.fetch();
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
            var ldr = _.template($("#loader-tmpl").html());
            $(this.childContainer).append(ldr({ colspan: this.columnCount }));
        },
        renderChildren: function () {
            $(this.childContainer).empty();
            var tmpl = _.template($("#job-step-detail-tmpl").html());
            if (this.collection.length === 0) {
                var t = _.template($("#noresults-tmpl").html());
                $(this.childContainer).append(t({ colspan: this.columnCount }));
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
        onRefreshClick: function () {
            _log("Refreshing Records");
            this.collection.fetch();
        },
        onRowClick: function (e) {
            var tr = $(e.target).closest("tr");
            var m = tr.data("model");
            if (m !== undefined || m !== null) {
                tr.siblings().removeClass("info");
                tr.addClass("info");
                this.currrentSelected = this.collection.get(m.id);
                $(".btn-moveup").prop("disabled", (m.stepId <= 1));
                $(".btn-movedown").prop("disabled", (m.stepId >= this.collection.length));
            }
        },
        onCreateStepClick: function () {
            var m = new Models.JobStep({ jobId: this.model.get("id"), stepId: this.collection.length + 1 });
            new Views.JobStepEditor({ model: m, collection: this.collection }).showDialog();
        },
        onEditStepClick: function () {
            if (this.currrentSelected === null) {
                _log("No currently selected record.");
            }
            new Views.JobStepEditor({ model: this.currrentSelected }).showDialog();
        },
        onDeleteStepClick: function () {
            if (this.currrentSelected === null) {
                _log("No currently selected record.");
            }
            new Views.JobStepConfirmDelete({ model: this.currrentSelected, collection: this.collection }).showDialog();
        },
        onMoveUpClick: function () {
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
        onMoveDownClick: function () {
            if (this.currrentSelected === null) {
                return;
            }
            var m = this.currrentSelected;
            var mdls = this.collection;
            var index = mdls.indexOf(this.currrentSelected);
            if (index < mdls.length) {
                mdls.remove(m, { silen: true });
                mdls.add(m, { at: index + 1 });
            }
        },
        events: {
            "click .btn-refresh": "onRefreshClick",
            "click tbody tr": "onRowClick",
            "click .btn-newStep": "onCreateStepClick",
            "click .btn-editStep": "onEditStepClick",
            "click .btn-deleteStep": "onDeleteStepClick",
            "click .btn-moveup": "onMoveUpClick",
            "click .btn-movedown": "onMoveDownClick"
        }
    });

    Views.JobStepEditor = Backbone.View.extend({
        viewName: "JobStepEditor",
        className: "modal fade",
        template: _.template($("#jobstep-edit-tmpl").html()),
        koViewModel: null,
        initialize: function () {
            _log("Initializing " + this.viewName);
            this.koViewModel = new ViewModels.JobStep();
            this.render();
        },
        render: function () {
            this.$el.empty();
            var jm = this.model.toJSON();
            this.koViewModel.setData(jm);

            this.$el.append(this.template(jm));
            this.$el.appendTo("body");

            //    Apply Knockout Binding to the Element
            ko.applyBindings(this.koViewModel, this.el);
            //    Show the Dialog
            this.showDialog();
            //    Return this
            return this;
        },
        showDialog: function () {
            this.$el.modal({
                backdrop: "static",
                keyboard: false,
                show: true
            });
        },
        OnClose: function () {
            this.$el.modal('hide');
        },
        OnSave: function () {
            var _slf = this;
            $(".btn-save").button("loading");
            _log("Saving Job Step");
            var m = this.koViewModel.toJS();
            this.model.set(m, { silent: true });
            var resp = this.model.save();
            resp.done(function () {
                if (_slf.collection !== undefined) {
                    _slf.collection.add(_slf.model);
                }

                _slf.remove();
            });
            resp.always(function () {
                $(".btn-save").button("reset");
            });
        },
        remove: function () {
            ko.cleanNode(this.el);
            Backbone.View.prototype.remove.call(this);
        },
        events: {
            "hidden.bs.modal": "remove",
            "click .btn-cancel": "OnClose",
            "click .btn-save": "OnSave"
        }
    });

    Views.JobStepConfirmDelete = Backbone.View.extend({
        viewName: "JobDelete",
        className: "modal fade",
        template: _.template($("#jobstep-confirm-delete-tmpl").html()),
        koViewModel: null,
        initialize: function (options) {
            _log("Initializing " + this.viewName);
            this.koViewModel = new ViewModels.JobStepConfirmDelete();
            this.render();
        },
        render: function () {
            this.$el.empty();
            var jm = this.model.toJSON();
            this.koViewModel.setData(jm);

            this.$el.append(this.template(jm));
            this.$el.appendTo("body");
            this.showDialog();
            //    Apply Knockout Binding to the Element
            ko.applyBindings(this.koViewModel, this.el);
            return this;
        },
        showDialog: function () {
            this.$el.modal({ backdrop: "static", keyboard: false, show: true });
        },
        onAcceptClick: function () {
            var _slf = this;
            //    Handle the Deletion
            var resp = this.model.destroy({ wait: true });
            resp.done(function () {
                _slf.collection.remove(_slf.model);
                _slf.OnClose();
            });
        },
        remove: function () {
            ko.cleanNode(this.el);
            Backbone.View.prototype.remove.call(this);
        },
        OnClose: function () {
            this.$el.modal('hide');
        },
        events: {
            "hidden.bs.modal": "remove",
            "click .btn-cancel": "OnClose",
            "click .btn-accept": "onAcceptClick"
        }
    });

    Views.JobSchedules = Backbone.View.extend({
        viewName: "JobSchedules",
        elName: "#job-content",
        childContainer: "#tbl-body",
        columnCount: 3,
        currrentSelected: null,
        template: _.template($("#job-schedules-tmpl").html()),
        initialize: function () {
            _log("Initializing " + this.viewName);
            this.collection = new Collections.JobSchedules({ jobId: this.model.get("id") });

            this.listenTo(this.collection, "request", this.renderLoading);
            this.listenTo(this.collection, "sync", this.renderChildren);

            this.collection.fetch();
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
            var ldr = _.template($("#loader-tmpl").html());
            $(this.childContainer).append(ldr({ colspan: this.columnCount }));
        },
        renderChildren: function () {
            _log("Rendering Children");
            $(this.childContainer).empty();
            var tmpl = _.template($("#job-schedule-detail-tmpl").html());
            if (this.collection.length === 0) {
                var t = _.template($("#noresults-tmpl").html());
                $(this.childContainer).append(t({ colspan: this.columnCount }));
            } else {
                _log("Rendering Children: Collection");
                this.collection.forEach(function (item) {
                    var m = item.toJSON();
                    var r = tmpl(m);
                    var x = $(r).data("model", m);
                    $(this.childContainer).append(x);
                }, this);
                $(this.childContainer + ">tr:first").click();
            }
        },
        onRowClick: function (e) {
            var tr = $(e.target).closest("tr");
            var m = tr.data("model");
            if (m !== undefined || m !== null) {
                tr.siblings().removeClass("info");
                tr.addClass("info");
                this.currrentSelected = this.collection.get(m.id);
            }
        },
        onRefreshClick: function () {
            _log("Refreshing Records");
            this.collection.fetch();
        },
        onCreateScheduleClick: function () {
            var m = new Models.JobSchedule({ jobId: this.model.get("id") });
            new Views.JobScheduleEditor({ model: m, collection: this.collection }).showDialog();
        },
        onEditScheduleClick: function () {
            if (this.currrentSelected === null) {
                _log("No currently selected record.");
            }
            new Views.JobScheduleEditor({ model: this.currrentSelected }).showDialog();
        },
        onDeleteScheduleClick: function () {
            if (this.currrentSelected === null) {
                _log("No currently selected record.");
            }
            new Views.JobScheduleConfirmDelete({ model: this.currrentSelected, collection: this.collection }).showDialog();
        },
        events: {
            "click .btn-refresh": "onRefreshClick",
            "click tbody tr": "onRowClick",
            "click .btn-newSchedule": "onCreateScheduleClick",
            "click .btn-editSchedule": "onEditScheduleClick",
            "click .btn-deleteSchedule": "onDeleteScheduleClick",
        }
    });

    Views.JobScheduleEditor = Backbone.View.extend({
        viewName: "ScheduleEditor",
        className: "modal fade",
        template: _.template($("#schedule-edit-tmpl").html()),
        koViewModel: null,
        initialize: function (options) {
            _log("Initializing " + this.viewName);
            if (this.model) {
                this.koViewModel = new ViewModels.Schedule(this.model.toJSON());
            }
            this.render();
        },
        render: function () {
            this.$el.empty();
            var jm = this.model.toJSON();
            this.$el.append(this.template(jm));
            this.$el.appendTo("body");
            this.showDialog();
            //    Apply Knockout Binding to the Element
            ko.applyBindings(this.koViewModel, this.el);
            return this;
        },
        showDialog: function () {
            this.$el.modal({ backdrop: "static", keyboard: false, show: true });
        },
        OnClose: function () {
            this.$el.modal('hide');
        },
        OnSave: function () {
            var _slf = this;
            $(".btn-save").button("loading");
            var s = this.koViewModel.toJS();
            this.model.set(s, { silent: true });
            var resp = this.model.save();
            resp.done(function () {
                if (this.collection !== undefined) {
                    this.collection.add(this.model);
                }
                _slf.OnClose();
            });
            resp.always(function () {
                $(".btn-save").button("reset");
            });
        },
        remove: function () {
            //    remove Knockout Binding
            ko.cleanNode(this.el);
            Backbone.View.prototype.remove.call(this);
        },
        events: {
            "hidden.bs.modal": "remove",
            "click .btn-cancel": "OnClose",
            "click .btn-save": "OnSave"
        }
    });

    Views.JobScheduleConfirmDelete = Backbone.View.extend({
        viewName: "ScheduleConfirmDelete",
        className: "modal fade",
        template: _.template($("#schedule-confirm-delete-tmpl").html()),
        koViewModel: null,
        initialize: function (options) {
            _log("Initializing " + this.viewName);
            this.koViewModel = new ViewModels.JobScheduleConfirmDelete();
            this.render();
        },
        render: function () {
            this.$el.empty();
            var jm = this.model.toJSON();
            this.koViewModel.setData(jm);

            this.$el.append(this.template(jm));
            this.$el.appendTo("body");
            this.showDialog();
            //    Apply Knockout Binding to the Element
            ko.applyBindings(this.koViewModel, this.el);
            return this;
        },
        remove: function () {
            ko.cleanNode(this.el);
            Backbone.View.prototype.remove.call(this);
        },
        showDialog: function () {
            this.$el.modal({ backdrop: "static", keyboard: false, show: true });
        },
        onAcceptClick: function () {
            var _slf = this;
            //    Handle the Deletion
            var resp = this.model.destroy({ wait: true });
            resp.done(function () {
                _slf.collection.remove(_slf.model);
                _slf.OnClose();
            });
        },
        OnClose: function () {
            this.$el.modal('hide');
        },
        events: {
            "hidden.bs.modal": "remove",
            "click .btn-cancel": "OnClose",
            "click .btn-accept": "onAcceptClick"
        }
    });

    Views.JobHistory = Backbone.View.extend({
        viewName: "JobHistory",
        elName: "#main-content",
        childContainer: "#tbl-body",
        pagerContainer: "#results-pager",
        columnCount: 6,
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
            $(this.childContainer).append(ldr({ colspan: this.columnCount }));
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

            if (this.collection.length === 0) {
                var t = _.template($("#noresults-tmpl").html());
                $(this.childContainer).append(t({ colspan: this.columnCount }));
            } else {

                _slf.collection.forEach(function (item) {
                    var m = item.toJSON();
                    var r = tmpl(m);
                    $(_slf.childContainer).append(r);
                }, this);
                _slf.renderPager(_slf.collection.totalPages, _slf.model.get("page"));
            }
        },
        searchHistory: function () {
            var _slf = this;
            var m = _slf.model.toJSON();
            _slf.collection.fetch({ data: m });
        },
        renderPager: function (pageCount, pageNumber) {
            var _slf = this;
            var container = $(_slf.pagerContainer);
            container.empty();

            if (pageCount <= 1) {
                return;
            }

            var pager = $("<ul>").addClass("pagination pagination-sm pull-right").css("margin", "4px, 0px !important");
            if (isNaN(pageNumber)) {
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
            "click .btn-refresh": "searchHistory"
        }
    });

    var Router = Backbone.Router.extend({
        routes: {
            "": "defaultRoute",
            "jobs(/)": "defaultRoute",
            "jobs/edit/:id": "editJob",
            "jobs/history/:id(/:page)": "jobHistory",
            "jobs/steps/:id": "jobSteps",
            "jobs/schedules/:id": "jobSchedules"
        },
        defaultRoute: function () {
            _log("Default Route");
            if (m_currentView !== null) {
                m_currentView.remove();
            }
            m_currentView = new Views.JobActivity();
        },
        editJob: function (id) {
            _log("View Job Route " + id);
            if (m_currentView !== null) {
                m_currentView.remove();
            }
            var job = new Models.Job({ id: id });
            var resp = job.fetch();
            resp.done(function () {
                m_currentView = new Views.JobEdit({ model: job, childViewName: "" });
            });
        },
        jobHistory: function (id, page) {
            if (page === undefined || page === null || isNaN(page)) {
                page = 1;
            }
            _log("Job History Route id: [" + id + "] page: [" + page + "]");
            if (m_currentView !== null) {
                m_currentView.remove();
            }
            var historySearch = new Models.JobHistorySearch({ page: page });
            var job = new Models.Job({ id: id });
            var resp = job.fetch();
            resp.done(function () {
                historySearch.set("jobId", job.get("id"));
                historySearch.set("name", job.get("name"));
                m_currentView = new Views.JobHistory({ model: historySearch });
            });
        },
        jobSteps: function (id) {
            _log("Job Steps Route");
            if (m_currentView !== null) {
                m_currentView.remove();
            }
            var job = new Models.Job({ id: id });
            var resp = job.fetch();
            resp.done(function () {
                m_currentView = new Views.JobEdit({ model: job, childViewName: "JobSteps" });
            });

        },
        jobSchedules: function (id) {
            if (m_currentView !== null) {
                m_currentView.remove();
            }
            var job = new Models.Job({ id: id });
            var resp = job.fetch();
            resp.done(function () {
                m_currentView = new Views.JobEdit({ model: job, childViewName: "JobSchedules" });
            });
        }
    });

    var initialize = (function () {
        _log("Initializing Scheduler Application");
        m_subSystems = new Collections.SubSystems();
        m_jobActivityCollection = new Collections.JobActivity();
        m_jobActivityCollection.fetch();
        m_subSystems.fetch();


        m_router = new Router();
        //    Start the Backbone history a necessary step for bookmark-able URL's
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
            if (xhr !== null) {
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
})(window, $, ko, _, Backbone);

$(function () {
    "use strict";
    //    Disable caching for ajax requests
    $.ajaxSetup({ cache: false });

    //    Global Handler for href=# links
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
        };
        if (jqXHR.status) {
            title = statusErrorMap[jqXHR.status];
            if (!title) {
                title = "Unknown Error";
            }
        } else if (event == "parsererror") {
            title = "Failed to Parse JSON Response";
        } else if (event == "timeout") {
            title = "Request Time out";
        } else if (event == 'abort') {
            title = "Request was aborted by the server";
        } else {
            title = "Unknown Error";
        }
        window.app.showError(title, jqXHR.statusText, jqXHR.status, jqXHR);
    });

    window.onerror = function (errorMsg, url, lineNumber, column, errorObj) {
        window.app.showError("JavaScript Error", errorMsg, "JavaScript Error", null);
        //    Return true to tell the browser you've handled the error yourself
        //    or Return false to let the browser run its error handler as well
        return false;
    };
});
