using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.LocalNotifications;
using System.Runtime.InteropServices;

namespace MobileApp
{
    [Table("Terms")]

    public class Term
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    [Table("Courses")]

    public class Course
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public int Term { get; set; }
        public string CourseName { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string InstructorName { get; set; }
        public string InstructorPhone { get; set; }
        public string InstructorEmail { get; set; }
        public string Notes { get; set; }
        public int NotificationEnabled { get; set; }
    }

    [Table("Assessments")]
    public class Assessment
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Type { get; set; }
        public int Course { get; set; }
        public int NotificationEnabled { get; set; }
    }

   [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private SQLiteAsyncConnection _conn;
        public ObservableCollection<Term> _termList;
        private bool pushNotification = true;
        public MainPage()
        {
            InitializeComponent();
            _conn = DependencyService.Get<ISQLiteDb>().GetConnection();
            termListView.ItemTapped += new EventHandler<ItemTappedEventArgs>(Term_Click);
        }

        protected override async void OnAppearing()
        {
            await _conn.CreateTableAsync<Term>();
            await _conn.CreateTableAsync<Course>();
            await _conn.CreateTableAsync<Assessment>();

            var termList = await _conn.Table<Term>().ToListAsync();
            var courseList = await _conn.Table<Course>().ToListAsync();
            var assessmentList = await _conn.Table<Assessment>().ToListAsync();

            //populate app with dummy data if not data exists.
            if (!termList.Any())
            {
                var Term1 = new Term();
                Term1.Title = "Term 1";
                Term1.StartDate = new DateTime(2023, 03, 01);
                Term1.EndDate = new DateTime(2023, 08, 31);
                await _conn.InsertAsync(Term1);
                termList.Add(Term1);

                //Course 1
                var Course1 = new Course();
                Course1.CourseName = "Mobile Application Development";
                Course1.StartDate = new DateTime(2023, 06, 10);
                Course1.StartDate = new DateTime(2023, 06, 17);
                Course1.Status = "In-Progress";
                Course1.InstructorName = "Stephanie Merrington";
                Course1.InstructorPhone = "555-555-5555";
                Course1.InstructorEmail = "smerr79@wgu.edu";
                Course1.NotificationEnabled = 1;
                Course1.Notes = "This course is fun!";
                Course1.Term = Course1.Id;
                await _conn.InsertAsync(Course1);

                var ObjectiveAssessment1 = new Assessment();
                ObjectiveAssessment1.Title = "OA";
                ObjectiveAssessment1.StartDate = new DateTime(2023, 06, 10);
                ObjectiveAssessment1.EndDate = new DateTime(2023, 06, 11);
                ObjectiveAssessment1.Course = Course1.Id;
                ObjectiveAssessment1.Type = "Objective";
                ObjectiveAssessment1.NotificationEnabled = 1;
                await _conn.InsertAsync(ObjectiveAssessment1);

                var PerformanceAssessment1 = new Assessment();
                PerformanceAssessment1.Title = "PA";
                PerformanceAssessment1.StartDate = new DateTime(2023, 06, 10);
                PerformanceAssessment1.EndDate = new DateTime(2023, 06, 11);
                PerformanceAssessment1.Course = Course1.Id;
                PerformanceAssessment1.Type = "Performance";
                PerformanceAssessment1.NotificationEnabled = 1;
                await _conn.InsertAsync(PerformanceAssessment1);
            }

            //notification handling
            if (pushNotification == true)
            {
                pushNotification = false;
                int courseId = 0;
                foreach (Course course in courseList)
                {
                    courseId++;
                    if (course.NotificationEnabled == 1)
                    {
                        if (course.StartDate == DateTime.Today)
                            CrossLocalNotifications.Current.Show("Reminder", $"{course.CourseName} begins today!", courseId);
                        if (course.EndDate == DateTime.Today)
                            CrossLocalNotifications.Current.Show("Reminder", $"{course.CourseName} ends today!", courseId);
                    }
                }

                int assessmentId = courseId;
                foreach (Assessment assessment in assessmentList)
                {
                    assessmentId++;
                    if (assessment.NotificationEnabled == 1)
                    {
                        if (assessment.StartDate == DateTime.Today)
                            CrossLocalNotifications.Current.Show("Reminder", $"{assessment.Title} begins today!", assessmentId);
                        if (assessment.EndDate == DateTime.Today)
                            CrossLocalNotifications.Current.Show("Reminder", $"{assessment.Title} ends today!", assessmentId);
                    }
                }
            }

            _termList = new ObservableCollection<Term>(termList);
            termListView.ItemsSource = _termList;
            base.OnAppearing();
        }

        private async void OnButtonClick(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new AddTerm(this));
        }
        
        async private void Term_Click(object sender, ItemTappedEventArgs e)
        {
            Term term = (Term)e.Item;
            await Navigation.PushModalAsync(new TermDetail(term));
        }
    }
}
