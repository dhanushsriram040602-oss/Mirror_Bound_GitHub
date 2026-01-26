═══════════════════════════════════════════════════════════════
    ⭐ STAR RATING SYSTEM - QUICK START GUIDE ⭐
═══════════════════════════════════════════════════════════════

SYSTEM OVERVIEW:
Players earn 1-3 stars per level based ONLY on completion time.
  • Complete faster = More stars
  • Simple and easy to understand
  • No coin requirements

═══════════════════════════════════════════════════════════════

🛠️ SETUP WIZARD (RECOMMENDED):

1. In Unity menu: Tools → Star Rating System → Setup Guide
2. Use the helper window buttons for easy setup!

═══════════════════════════════════════════════════════════════

📋 MANUAL SETUP (5 STEPS):

STEP 1: Add to Each Level Scene
  ► Open level scene (e.g., Level_1)
  ► Menu: Tools → Star Rating System → Add to Current Scene
  ► OR manually: Create Empty → Add Component → StarRatingSystem

STEP 2: Configure Level Star Thresholds
  ► Find LevelSettings GameObject in your scene
  ► Set in Inspector:
     • Three Star Time: 15.0 (seconds to get 3 stars)
     • Two Star Time: 30.0 (seconds to get 2 stars)
     • Anything slower = 1 star

  That's it! No coin counting needed.

STEP 3: Setup Win Panel UI
  ► Open Win Panel in scene/prefab
  ► Add 3 Image UI elements (children of WinPanel)
  ► Name them: Star1, Star2, Star3
  ► Add star sprite to each
  ► Select WinMenu GameObject
  ► In Inspector, drag Star1/2/3 into "Star Images" array
  ► Set colors:
     - Star Filled Color: Yellow (RGB: 255, 255, 0)
     - Star Empty Color: Gray (RGB: 77, 77, 77, alpha: 128)

  That's all! Clean and simple.

STEP 4: Update Level Button Prefab
  ► Open Level Button prefab
  ► Add 3 Image children
  ► Name them: Star1, Star2, Star3 (MUST contain "Star")
  ► Position below level number
  ► Add star sprite to each
  ► Done! (Script finds them automatically by name)

STEP 5: Configure Level Select Menu
  ► Find LevelSelectMenu GameObject
  ► In Inspector, set:
     - Star Filled Color: Yellow
     - Star Empty Color: Gray
  ► OPTIONAL: Add TextMeshPro "Total Stars Text" to show total

═══════════════════════════════════════════════════════════════

🎮 HOW IT WORKS:

Star Calculation (Time-Based Only):
  3 STARS = Finish under "Three Star Time"
  2 STARS = Finish under "Two Star Time"
  1 STAR  = Complete the level (any time)

Example:
  Level Settings:
    - Three Star Time: 15s
    - Two Star Time: 30s

  Results:
    - Finish in 12s → 3 STARS ⭐⭐⭐
    - Finish in 25s → 2 STARS ⭐⭐
    - Finish in 35s → 1 STAR ⭐
    - Finish in 90s → 1 STAR ⭐

  Simple! Just focus on speed.

═══════════════════════════════════════════════════════════════

⚙️ RECOMMENDED TIME THRESHOLDS:

Easy Levels (World 1):
  Level 1:  3★ = 15s | 2★ = 30s
  Level 2:  3★ = 18s | 2★ = 35s
  Level 3:  3★ = 20s | 2★ = 40s
  Level 4:  3★ = 25s | 2★ = 50s
  Level 5:  3★ = 30s | 2★ = 60s

Medium Levels (World 2):
  Level 11: 3★ = 35s | 2★ = 70s
  Level 12: 3★ = 40s | 2★ = 80s

Hard Levels (Future):
  Level 21: 3★ = 50s | 2★ = 100s

TIP: Play each level yourself 2-3 times:
  1. First playthrough = note the time
  2. Optimized run = set as 3-star time
  3. Add 50% more time = set as 2-star time

═══════════════════════════════════════════════════════════════

🎨 UI CUSTOMIZATION:

Star Colors:
  ► Change in WinMenu component:
     - Star Filled Color (default: yellow)
     - Star Empty Color (default: gray transparent)

  ► Change in LevelSelectMenu component:
     - Same color fields

Star Sprites:
  ► Use any sprite you want for stars
  ► Recommended size: 64x64 pixels
  ► Can use filled/outlined variations

Optional Timer Display:
  ► In gameplay level Canvas:
  ► Add TextMeshPro text named "TimerText"
  ► UIManager auto-finds and updates it!
  ► Shows: "Time: 12.3s" during gameplay

═══════════════════════════════════════════════════════════════

✅ TESTING CHECKLIST:

□ StarRatingSystem exists in gameplay scenes
□ LevelSettings configured with thresholds
□ Coin count matches actual coins in level
□ Win panel shows 3 star images
□ Complete level → stars display correctly
□ Return to level select → stars appear on button
□ Replay and beat time → "NEW BEST!" appears
□ Exit Play Mode → stars persist (PlayerPrefs)
□ Level buttons show correct star colors

═══════════════════════════════════════════════════════════════

🐛 TROUBLESHOOTING:

Problem: Stars not showing on win screen
  → Check WinMenu has starImages array assigned (size 3)
  → Verify Star1, Star2, Star3 Images are assigned

Problem: Stars not showing on level buttons
  → Button prefab needs 3 Images with "Star" in name
  → Check LevelSelectMenu has star colors set

Problem: Timer not updating in-game
  → Add TextMeshPro named "TimerText" to Canvas
  → Check UIManager has Update() method

Problem: "NEW BEST!" always showing
  → This is correct if it's your first completion!
  → Replay to set a baseline, then beat it

Problem: Stars not saving between sessions
  → Check LevelManager is marked DontDestroyOnLoad
  → Verify WinMenu calls SaveProgress()
  → Test in Build (not just Editor)

═══════════════════════════════════════════════════════════════

🔧 USEFUL MENU COMMANDS:

Tools → Star Rating System → Setup Guide
  Opens visual setup wizard

Tools → Star Rating System → Add to Current Scene
  Adds StarRatingSystem GameObject

Tools → Star Rating System → Count Coins in Scene
  Auto-counts coins and updates LevelSettings

═══════════════════════════════════════════════════════════════

💾 DATA STORAGE:

Stars saved in PlayerPrefs as:
  "Level_1_Stars" = 3
  "Level_2_Stars" = 2
  "Level_X_Stars" = Y

To reset ALL progress:
  Tools → Star Rating System → Reset All Progress

Or in code:
  LevelManager.Instance.ResetProgress();

═══════════════════════════════════════════════════════════════

📊 ACCESSING STAR DATA IN CODE:

Get stars for specific level:
  int stars = LevelManager.Instance.GetLevelStars(levelIndex);

Get total stars across all levels:
  int total = LevelManager.Instance.GetTotalStars();

Check if player earned 3 stars:
  if (LevelManager.Instance.GetLevelStars(5) == 3)
  {
      UnlockBonusContent();
  }

Unlock content based on total stars:
  if (LevelManager.Instance.GetTotalStars() >= 30)
  {
      UnlockWorld2();
  }

═══════════════════════════════════════════════════════════════

🎯 NEXT STEPS:

1. Setup all your levels with star thresholds
2. Test each level to verify times are fair
3. Add visual polish (star animations, particles)
4. Add sound effects for earning stars
5. Create achievements (All 3-Stars on World 1, etc.)
6. Add leaderboards integration
7. Use total stars to unlock bonus content

═══════════════════════════════════════════════════════════════

📝 IMPLEMENTATION DETAILS:

Files Modified:
  ✓ StarRatingSystem.cs (new)
  ✓ LevelSettings.cs (updated)
  ✓ LevelManager.cs (updated)
  ✓ WinMenu.cs (updated)
  ✓ LevelSelectMenu.cs (updated)
  ✓ UIManager.cs (updated)
  ✓ Coin.cs (updated)
  ✓ StarSystemSetupHelper.cs (editor tool)

Performance Impact: Minimal
  - Single Update() call for timer display
  - PlayerPrefs save on level completion
  - No allocations during gameplay

═══════════════════════════════════════════════════════════════

Need help? Check the Setup Guide window:
  Tools → Star Rating System → Setup Guide

Good luck with your game! ⭐⭐⭐
