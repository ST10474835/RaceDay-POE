
        [Required, Range(0.01, 999.99)]
        public decimal? Distance { get; set; }

        [Required]
        [RegularExpression("^(Run|Walk|Cycle)$",
            ErrorMessage = "EventType must be 'Run', 'Walk' or 'Cycle'.")]
        public string EventType { get; set; } = string.Empty;
    }
}
