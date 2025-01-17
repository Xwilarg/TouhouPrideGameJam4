﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using TouhouPrideGameJam4.Character.Player;
using TouhouPrideGameJam4.Dialog.Parsing;
using TouhouPrideGameJam4.Game;
using TouhouPrideGameJam4.Game.Persistency;
using TouhouPrideGameJam4.Map;
using TouhouPrideGameJam4.SO.Character;
using TouhouPrideGameJam4.Sound;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TouhouPrideGameJam4.Dialog
{
    public class StoryManager : MonoBehaviour
    {
        public static StoryManager Instance { get; private set; }

        [SerializeField]
        private TMP_Text _vnName, _vnContent;

        [SerializeField]
        private Image _vnImage;

        [SerializeField]
        private GameObject _vnContainer, _choiceContainer;

        [SerializeField]
        private TextAsset _introDialog, _mountain1, _mountain2, _forest1, _forest2, _endQuestAya, _endQuestReimu, _questAya, _questReimu, _gameOver, _sdm1, _sdm1Aya, _sdm1Reimu,
            _sdm4, _sdmDoor1Aya, _sdmDoor2Aya, _sdmDoor3Aya, _sdmDoor1Reimu, _sdmDoor2Reimu, _sdmDoor3Reimu, _sdmBoss, _endingAya, _endingReimu, _ending2Aya, _ending2Reimu;
        private DialogStatement[] _introStatement, _mountain1Statement, _mountain2Statement, _forest1Statement, _forest2Statement, _endQuestAyaStatement,
            _endQuestReimuStatement, _questAyaStatement, _questReimuStatement, _gameOverStatement, _sdm1Statement, _sdm1AyaStatement, _sdm1ReimuStatement, _sdm4Statement,
            _sdmDoor1AyaStatement, _sdmDoor2AyaStatement, _sdmDoor3AyaStatement,
            _sdmDoor1ReimuStatement, _sdmDoor2ReimuStatement, _sdmDoor3ReimuStatement, _sdmBossStatement, _endingAyaStatement, _endingReimuStatement,
            _ending2AyaStatement, _ending2ReimuStatement;

        private DialogStatement[] _current;
        private int _index;

        [SerializeField]
        private VNCharacterInfo[] _characters;

        [SerializeField]
        private GameObject _skipIcon;

        private AudioSource _source;

        [SerializeField]
        private GameObject _cgAya, _cgReimu, _cgShrine;

        [SerializeField]
        private AudioClip _bgmEnd;

        private void Awake()
        {
            _vnContainer.SetActive(false);
            _choiceContainer.SetActive(false);

            Instance = this;
            _source = GetComponent<AudioSource>();
        }

        private void Start()
        {
            var c = _characters.ToList();
            c.Add(new()
            {
                Name = "???",
                Key = "???",
                Color = Color.black
            });
            _characters = c.ToArray();

            ParseAllStories();
            ProgressIsAvailable(StoryProgress.Intro);
            if (MapManager.Instance != null)
            {
                ProgressIsAvailable(StoryProgress.YoukaiMountain1);
            }
        }

        public void ParseAllStories()
        {
            _introStatement = Parse(_introDialog);
            _mountain1Statement = Parse(_mountain1);
            _mountain2Statement = Parse(_mountain2);
            _forest1Statement = Parse(_forest1);
            _forest2Statement = Parse(_forest2);
            _endQuestAyaStatement = Parse(_endQuestAya);
            _endQuestReimuStatement = Parse(_endQuestReimu);
            _questAyaStatement = Parse(_questAya);
            _questReimuStatement = Parse(_questReimu);
            _gameOverStatement = Parse(_gameOver);
            _sdm1Statement = Parse(_sdm1);
            _sdm1AyaStatement = Parse(_sdm1Aya);
            _sdm1ReimuStatement = Parse(_sdm1Reimu);
            _sdm4Statement = Parse(_sdm4);
            _sdmDoor1AyaStatement = Parse(_sdmDoor1Aya);
            _sdmDoor2AyaStatement = Parse(_sdmDoor2Aya);
            _sdmDoor3AyaStatement = Parse(_sdmDoor3Aya);
            _sdmDoor1ReimuStatement = Parse(_sdmDoor1Reimu);
            _sdmDoor2ReimuStatement = Parse(_sdmDoor2Reimu);
            _sdmDoor3ReimuStatement = Parse(_sdmDoor3Reimu);
            _sdmBossStatement = Parse(_sdmBoss);
            _endingAyaStatement = Parse(_endingAya);
            _endingReimuStatement = Parse(_endingReimu);
            _ending2AyaStatement = Parse(_ending2Aya);
            _ending2ReimuStatement = Parse(_ending2Reimu);
        }

        private bool _isGameOver;

        public void ShowGameOver()
        {
            _isGameOver = true;
            ReadDialogues(_gameOverStatement);
        }

        public void DisplayReimuQuest()
        {
            _choiceContainer.SetActive(false);
            PersistencyManager.Instance.QuestStatus = QuestStatus.PendingReimu;
            TurnManager.Instance.UpdateObjectiveText();
            ProgressIsAvailable(StoryProgress.Quest);
        }

        public void DisplayAyaQuest()
        {
            _choiceContainer.SetActive(false);
            PersistencyManager.Instance.QuestStatus = QuestStatus.PendingAya;
            TurnManager.Instance.UpdateObjectiveText();
            ProgressIsAvailable(StoryProgress.Quest);
        }

        private bool _isSkipping;
        public void ToggleSkipDialogs()
        {
            _isSkipping = !_isSkipping;
            _skipIcon.SetActive(_isSkipping);
            if (_isSkipping)
            {
                StartCoroutine(SkipDialogues());
            }
            else
            {
                lock (_vnContent)
                {
                    _vnContent.text = _current[_index - 1].Content;
                }
            }
        }

        public void GoBack()
        {
            if (_index > 1)
            {
                _index--;
                _vnContent.text = _current[_index - 1].Content[..^1];
                ShowNextDialogue();
            }
        }

        private IEnumerator SkipDialogues()
        {
            while (_isSkipping)
            {
                ShowNextDialogue();
                yield return new WaitForSeconds(.1f);
            }
        }

        private bool _dontRevertRPGMode;
        public void EnableEndCinematic()
        {
            PlayerController.Instance.EnableVNController();
            _dontRevertRPGMode = true;
        }

        public void ShowShrine()
        {
            _cgShrine.gameObject.SetActive(true);
            BGMManager.Instance.SetSong(null, _bgmEnd);
        }

        public void ProgressIsAvailable(StoryProgress requirement)
        {
            if (PersistencyManager.Instance.StoryProgress == requirement)
            {
                ReadDialogues(requirement switch
                {
                    StoryProgress.Intro => _introStatement,
                    StoryProgress.YoukaiMountain1 => _mountain1Statement,
                    StoryProgress.YoukaiMountain1Half => _mountain2Statement,
                    StoryProgress.Forest1 => _forest1Statement,
                    StoryProgress.Quest => PersistencyManager.Instance.QuestStatus == QuestStatus.PendingReimu ? _questReimuStatement : _questAyaStatement,
                    StoryProgress.EndQuest => PersistencyManager.Instance.QuestStatus == QuestStatus.CompletedReimu ? _endQuestReimuStatement : _endQuestAyaStatement,
                    StoryProgress.Forest4Kill => _forest2Statement,
                    StoryProgress.SDM1 => _sdm1Statement,
                    StoryProgress.SDM1Part2 => PersistencyManager.Instance.QuestStatus == QuestStatus.CompletedReimu ? _sdm1ReimuStatement : _sdm1AyaStatement,
                    StoryProgress.SDM4 => _sdm4Statement,
                    StoryProgress.SDMDoor1 => PersistencyManager.Instance.QuestStatus == QuestStatus.CompletedReimu ? _sdmDoor1ReimuStatement : _sdmDoor1AyaStatement,
                    StoryProgress.SDMDoor2 => PersistencyManager.Instance.QuestStatus == QuestStatus.CompletedReimu ? _sdmDoor2ReimuStatement : _sdmDoor2AyaStatement,
                    StoryProgress.SDMDoor3 => PersistencyManager.Instance.QuestStatus == QuestStatus.CompletedReimu ? _sdmDoor3ReimuStatement : _sdmDoor3AyaStatement,
                    StoryProgress.Remilia => _sdmBossStatement,
                    StoryProgress.Ending => PersistencyManager.Instance.QuestStatus == QuestStatus.CompletedReimu ? _endingReimuStatement : _endingAyaStatement,
                    StoryProgress.EndingCG => PersistencyManager.Instance.QuestStatus == QuestStatus.CompletedReimu ? _ending2ReimuStatement : _ending2AyaStatement,
                    _ => throw new NotImplementedException()
                });
                PersistencyManager.Instance.IncreaseStory();
            }
        }

        public void ShowNextDialogue()
        {
            if (_current == null || _index == _current.Length) // End of VN part
            {
                if (_isGameOver)
                {
                    PersistencyManager.Instance.TotalEnergy += PlayerController.Instance.Energy;
                    SceneManager.LoadScene("MenuRuns");
                }
                else if (PersistencyManager.Instance.StoryProgress == StoryProgress.Quest)
                {
                    _choiceContainer.SetActive(true);
                }
                else if (PersistencyManager.Instance.StoryProgress == StoryProgress.SDM1)
                {
                    MapManager.Instance.GoToNextZone();
                }
                else if (PersistencyManager.Instance.StoryProgress == StoryProgress.SDM1Part2)
                {
                    ProgressIsAvailable(StoryProgress.SDM1Part2);
                }
                else if (PersistencyManager.Instance.StoryProgress == StoryProgress.EndingCG)
                {
                    if (PersistencyManager.Instance.QuestStatus == QuestStatus.CompletedReimu)
                    {
                        _cgReimu.SetActive(true);
                    }
                    else
                    {
                        _cgAya.SetActive(true);
                    }
                    ProgressIsAvailable(StoryProgress.EndingCG);
                }
                else if (PersistencyManager.Instance.StoryProgress == StoryProgress.Remilia)
                {
                    MapManager.Instance.GoToNextZone();
                }
                else if (PersistencyManager.Instance.StoryProgress == StoryProgress.Done)
                {
                    Destroy(PersistencyManager.Instance.gameObject);
                    SceneManager.LoadScene("MainMenu");
                }
                else if (!_dontRevertRPGMode)
                {
                    _vnContainer.SetActive(false);
                    PlayerController.Instance.EnableRPGController();
                    _current = null;
                    foreach (var button in GameObject.FindGameObjectsWithTag("MenuButton").Select(x => x.GetComponent<Button>()))
                    {
                        button.interactable = true;
                    }
                    _isSkipping = false;
                }
            }
            else if (_index > 0 && _vnContent.text.Length < _current[_index - 1].Content.Length)
            {
                lock(_vnContent)
                {
                    _vnContent.text = _current[_index - 1].Content;
                }
            }
            else
            {
                _vnName.text = _current[_index].Name;
                _vnName.color = _current[_index].Color;
                _vnContent.text = string.Empty;
                if (_current[_index].Image == null)
                {
                    _vnImage.gameObject.SetActive(false);
                }
                else
                {
                    _vnImage.gameObject.SetActive(true);
                    _vnImage.sprite = _current[_index].Image;
                }
                _index++;
                StartCoroutine(DisplayLetter());
            }
        }

        private IEnumerator DisplayLetter()
        {
            if (!_isSkipping)
            {
                _source.Play();
            }
            while (!_isSkipping && _current != null && _vnContent.text.Length < _current[_index - 1].Content.Length)
            {
                lock(_vnContent)
                {
                    if (_vnContent.text.Length < _current[_index - 1].Content.Length)
                    {
                        _vnContent.text = _current[_index - 1].Content[..(_vnContent.text.Length + 1)];
                    }
                }
                yield return new WaitForSeconds(.025f);
            }
            _source.Stop();
        }

        private void ReadDialogues(DialogStatement[] toRead)
        {
            PlayerController.Instance.EnableVNController();
            _vnContainer.SetActive(true);
            _current = toRead;
            _index = 0;
            ShowNextDialogue();
            foreach (var button in GameObject.FindGameObjectsWithTag("MenuButton").Select(x => x.GetComponent<Button>()))
            {
                button.interactable = false;
            }
        }

        private enum ParsingExpectation
        {
            Dialogue,
            Mood,
            Start,
            NewLine
        }

        private DialogStatement[] Parse(TextAsset file)
        {
            string text = file.text;

            List<DialogStatement> lines = new();
            Sprite targetMood = null;
            VNCharacterInfo currentCharacter = null;
            ParsingExpectation exp = ParsingExpectation.Start;

            foreach (var m in Regex.Matches(file.text, "[\\w?]+|\"[\\w\\s!?'’…,.()…‘’:-]*\"|\\n").Cast<Match>().Select(x => x.Value))
            {
                var match = m;
                if (m.StartsWith("\"")) match = match[1..];
                if (m.EndsWith("\"") && !m.EndsWith("\\\"")) match = match[..^1];
                if (match == "\n")
                {
                    if (exp == ParsingExpectation.NewLine || exp == ParsingExpectation.Start)
                    {
                        exp = ParsingExpectation.Start;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Parsing of {file.name} failed at {match} for state {exp}");
                    }
                }
                else
                {
                    var character = _characters.FirstOrDefault(x => x.Key == match.ToLowerInvariant());
                    if (match == "none" && exp == ParsingExpectation.Start) // Unset character
                    {
                        currentCharacter = null;
                        exp = ParsingExpectation.Dialogue;
                    }
                    else if (character != null && exp == ParsingExpectation.Start) // We are at the start and found a character info
                    {
                        currentCharacter = character;
                        exp = ParsingExpectation.Mood;
                    }
                    else if (exp == ParsingExpectation.Mood) // Next element, mood info
                    {
                        if (currentCharacter.Key != "???")
                        {
                            targetMood = match.ToLowerInvariant() switch
                            {
                                "joyful" => currentCharacter.JoyfulExpression,
                                "neutral" => currentCharacter.NeutralExpression,
                                "eyesclosed" => currentCharacter.EyesClosedExpression,
                                "angry" => currentCharacter.AngryExpression,
                                "surprised" => currentCharacter.SurprisedExpression,
                                "sad" => currentCharacter.SadExpression,
                                "shy" => currentCharacter.ShyExpression,
                                _ => throw new InvalidOperationException($"Invalid expression {match}")
                            };
                        }
                        else
                        {
                            targetMood = null;
                        }
                        exp = ParsingExpectation.Dialogue;
                    }
                    else if (character == null)
                    {
                        lines.Add(new()
                        {
                            Name = currentCharacter != null ? currentCharacter.Name : null,
                            Image = currentCharacter != null ? targetMood : null,
                            Content = match,
                            Color = currentCharacter != null ? currentCharacter.Color : Color.black
                        });
                        exp = ParsingExpectation.NewLine;
                    }
                    else
                    {
                        throw new System.InvalidOperationException($"Parsing of {file.name} failed at {match} for state {exp}");
                    }
                }
            }

            return lines.ToArray();
        }
    }
}
