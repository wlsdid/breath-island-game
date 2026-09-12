namespace BreathIsland
{
    public static class GameText
    {
        public static string Get(string key, bool korean)
        {
            return (key, korean) switch
            {
                ("welcome", true) => "숨결섬에 오신 것을 환영해요.", ("welcome", false) => "Welcome to Breath Island.",
                ("language", true) => "언어: 한국어", ("language", false) => "Language: English",
                ("saved", true) => "1번 슬롯에 저장했습니다.", ("saved", false) => "Saved to slot 1.",
                ("loaded", true) => "1번 슬롯을 불러왔습니다.", ("loaded", false) => "Loaded slot 1.",
                ("no_bait", true) => "미끼가 없어요.", ("no_bait", false) => "You are out of bait.",
                ("line_broke", true) => "줄이 끊어졌어요. 미끼만 소모됩니다.", ("line_broke", false) => "The line snapped. Only the bait was lost.",
                ("hooked", true) => "{0}이(가) 미끼를 물었어요!", ("hooked", false) => "A {0} took the bait!",
                ("predator", true) => "포식자 조우! 공격 신호를 보고 회피하세요.", ("predator", false) => "Predator! Watch the signal and dodge.",
                ("hit", true) => "꼬리치기에 맞았어요!", ("hit", false) => "The tail strike hit you!",
                ("washed_ashore", true) => "해안으로 돌아왔어요. 미끼 1개를 잃었습니다.", ("washed_ashore", false) => "You washed ashore and lost one bait.",
                ("sold", true) => "조개코인 {0}개를 받았어요.", ("sold", false) => "Received {0} shell coins.",
                ("displayed", true) => "수족관 전시 목록에 추가했어요.", ("displayed", false) => "Added to the aquarium display.",
                ("materials", true) => "재료 {0}개를 얻었어요.", ("materials", false) => "Received {0} materials.",
                ("recorded", true) => "도감을 자세히 기록했어요.", ("recorded", false) => "The field guide has been updated.",
                ("dodged", true) => "물보라를 피해 굴렀어요.", ("dodged", false) => "You rolled clear of the spray.",
                ("net", true) => "그물이 움직임을 늦췄어요.", ("net", false) => "The net slowed it down.",
                ("shock", true) => "진정 충격으로 기운을 뺐어요.", ("shock", false) => "The calming pulse weakened it.",
                ("gift_fail", true) => "선물을 살 코인이 부족해요.", ("gift_fail", false) => "Not enough coins for a gift.",
                ("gift_ok", true) => "호감도가 올랐어요.", ("gift_ok", false) => "Friendship increased.",
                ("upgrade_fail", true) => "업그레이드 재료가 부족해요.", ("upgrade_fail", false) => "Not enough upgrade materials.",
                ("upgrade_ok", true) => "시설이 성장했어요.", ("upgrade_ok", false) => "The facility has grown.",
                ("boat_unlocked", true) => "작은 보트가 완성됐어요!", ("boat_unlocked", false) => "The small boat is ready!",
                _ => key
            };
        }
    }
}
