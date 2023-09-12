﻿// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Prompts have a maximum length. Prompt lengths are be limited by model capacity or policy
/// PromptBuilder builds prompts consisting of multiple prompt sections in a way that the prompt
/// length does not exceeed a given maximum
/// </summary>
public class PromptBuilder
{
    Prompt _prompt;
    int _currentLength;
    int _maxLength;
    Func<string, int, string>? _substring;

    /// <summary>
    /// Create a builder to create prompts whose length does not exceed maxLength characters
    /// </summary>
    /// <param name="maxLength">maximum length</param>
    public PromptBuilder(int maxLength)
        : this(maxLength, Substring)
    {
    }

    /// <summary>
    /// Create a builder to create prompts whose length does not exceed maxLength characters
    /// If a full prompt section is too long, inovokes a substringExtractor callback to extract a
    /// suitable substring, if any. Substring extractors could do so at sentence boundaries, paragraph
    /// boundaries and so on.
    /// </summary>
    /// <param name="maxLength">Prompt will not exceed this maxLengthin characters</param>
    /// <param name="substringExtractor">optinal extractor</param>
    public PromptBuilder(int maxLength, Func<string, int, string>? substringExtractor = null)
    {
        _prompt = new Prompt();
        _maxLength = maxLength;
        _substring = substringExtractor;
    }

    /// <summary>
    /// The prompt being built
    /// </summary>
    public Prompt Prompt => _prompt;
    /// <summary>
    /// Current length of the prompt in characters
    /// </summary>
    public int Length => _currentLength;
    /// <summary>
    /// Maximum allowed prompt length
    /// </summary>
    public int MaxLength
    {
        get => _maxLength;
        set
        {
            if (value < _currentLength)
            {
                throw new ArgumentException($"CurrentLength: {_currentLength} exceeds {value}");
            }
            _maxLength = value;
        }
    }

    /// <summary>
    /// Add a prompt section if the total length of the prompt will not exceed limits
    /// </summary>
    /// <param name="text">text to add</param>
    /// <returns>true if added, false if not</returns>
    public bool Add(string text)
    {
        return Add(new PromptSection(text));
    }

    /// <summary>
    /// Add a prompt section if the total length of the prompt will not exceed limits
    /// </summary>
    /// <param name="section">section to add</param>
    /// <returns>true if added, false if not</returns>
    public bool Add(IPromptSection section)
    {
        if (section == null)
        {
            throw new ArgumentNullException(nameof(section));
        }

        string text = section.GetText();
        if (string.IsNullOrEmpty(text))
        {
            return true;
        }

        int lengthAvailable = _maxLength - _currentLength;
        if (text.Length <= lengthAvailable)
        {
            _prompt.Append(section);
            _currentLength += text.Length;
            return true;
        }
        if (_substring != null)
        {
            text = _substring(text, lengthAvailable);
            _prompt.Append(section.Source, text);
            return true;
        }
        return false;
    }

    public bool AddRange(IEnumerable<IPromptSection> sections)
    {
        if (sections == null)
        {
            throw new ArgumentNullException(nameof(sections));
        }
        foreach (var section in sections)
        {
            if (!Add(section))
            {
                return false;
            }
        }
        return true;
    }

    public void Clear()
    {
        _prompt.Clear();
        _currentLength = 0;
    }

    public void Reverse(int startAt, int count)
    {
        Reverse(startAt, count);
    }

    static string Substring(string text, int length)
    {
        return text.Substring(0, length);
    }
}