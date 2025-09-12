using System;
using System.Collections.Generic;
using System.Linq;
using Core.CliniCore.ClinicalDoc;

namespace CLI.CliniCore.Service.Editor
{
    /// <summary>
    /// Manages the state of the clinical document editor including
    /// current document, selection, cursor position, and view mode.
    /// </summary>
    public class EditorState
    {
        private readonly List<AbstractClinicalEntry> _flattenedEntries = new();
        private int _selectedIndex = 0;
        private bool _isDirty = false;

        public EditorState(ClinicalDocument document)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
            RefreshFlattenedEntries();
            ViewMode = EditorViewMode.Tree;
        }

        public enum EditorViewMode
        {
            Tree,       // Show hierarchical SOAP structure
            List,       // Show flat list of all entries
            Details     // Show detailed view of selected entry
        }

        public ClinicalDocument Document { get; }
        
        public EditorViewMode ViewMode { get; set; }
        
        public bool IsDirty 
        { 
            get => _isDirty;
            set => _isDirty = value;
        }

        public bool HasEntries => _flattenedEntries.Any();

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_flattenedEntries.Any())
                {
                    _selectedIndex = Math.Max(0, Math.Min(value, _flattenedEntries.Count - 1));
                }
                else
                {
                    _selectedIndex = 0;
                }
            }
        }

        public AbstractClinicalEntry? SelectedEntry
        {
            get
            {
                if (_flattenedEntries.Any() && _selectedIndex >= 0 && _selectedIndex < _flattenedEntries.Count)
                {
                    return _flattenedEntries[_selectedIndex];
                }
                return null;
            }
        }

        public IReadOnlyList<AbstractClinicalEntry> FlattenedEntries => _flattenedEntries.AsReadOnly();

        public void RefreshFlattenedEntries()
        {
            _flattenedEntries.Clear();
            _flattenedEntries.AddRange(Document.Entries.OrderBy(GetEntrySortOrder));
            
            // Ensure selected index is still valid
            if (_selectedIndex >= _flattenedEntries.Count)
            {
                _selectedIndex = Math.Max(0, _flattenedEntries.Count - 1);
            }
        }

        public void MoveSelectionUp()
        {
            SelectedIndex = SelectedIndex - 1;
        }

        public void MoveSelectionDown()
        {
            SelectedIndex = SelectedIndex + 1;
        }

        public void MoveToFirst()
        {
            SelectedIndex = 0;
        }

        public void MoveToLast()
        {
            SelectedIndex = Math.Max(0, _flattenedEntries.Count - 1);
        }

        public GroupedEntries GetGroupedEntries()
        {
            var subjective = _flattenedEntries.OfType<ObservationEntry>().Cast<AbstractClinicalEntry>().ToList();
            var objective = _flattenedEntries.Where(e => e is DiagnosisEntry || e is PrescriptionEntry).ToList();
            var assessment = _flattenedEntries.OfType<AssessmentEntry>().Cast<AbstractClinicalEntry>().ToList();
            var plan = _flattenedEntries.OfType<PlanEntry>().Cast<AbstractClinicalEntry>().ToList();

            return new GroupedEntries(subjective, objective, assessment, plan);
        }

        public int GetEntryIndex(AbstractClinicalEntry entry)
        {
            return _flattenedEntries.IndexOf(entry);
        }

        public void MarkDirty()
        {
            _isDirty = true;
        }

        public void MarkClean()
        {
            _isDirty = false;
        }

        private int GetEntrySortOrder(AbstractClinicalEntry entry)
        {
            // Order entries by SOAP methodology: S, O, A, P
            return entry switch
            {
                ObservationEntry => 1,      // Subjective
                DiagnosisEntry => 2,        // Objective  
                PrescriptionEntry => 3,     // Objective (linked to diagnosis)
                AssessmentEntry => 4,       // Assessment
                PlanEntry => 5,             // Plan
                _ => 6
            };
        }
    }

    /// <summary>
    /// Represents clinical entries grouped by SOAP methodology
    /// </summary>
    public record GroupedEntries(
        IList<AbstractClinicalEntry> Subjective,
        IList<AbstractClinicalEntry> Objective,
        IList<AbstractClinicalEntry> Assessment,
        IList<AbstractClinicalEntry> Plan
    )
    {
        public bool HasSubjective => Subjective.Any();
        public bool HasObjective => Objective.Any();
        public bool HasAssessment => Assessment.Any();
        public bool HasPlan => Plan.Any();
        
        public int TotalCount => Subjective.Count + Objective.Count + Assessment.Count + Plan.Count;
    }
}