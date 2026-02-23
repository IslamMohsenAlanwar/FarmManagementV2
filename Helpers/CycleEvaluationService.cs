using FarmManagement.API.Models;
using System.Linq;

namespace FarmManagement.API.Helpers
{
    public class EvaluationService
    {
        /// <summary>
        /// يحسب التقييم النهائي للدورة: مجموع النقاط لكل البنود عبر كل الأيام / مجموع النقاط القصوى لكل البنود عبر كل الأيام
        /// </summary>
        public double CalculateFinalScore(Cycle cycle)
        {
            if (cycle.Evaluations == null || !cycle.Evaluations.Any())
                return 0;

            double totalScore = 0;
            double maxScore = 0;

            foreach (var eval in cycle.Evaluations)
            {
                foreach (var detail in eval.Details)
                {
                    int itemMaxScore = detail.EvaluationItem?.MaxScore ?? 10; // لو مش محدد، افتراضياً 10
                    totalScore += detail.Score;
                    maxScore += itemMaxScore;
                }
            }

            if (maxScore == 0)
                return 0;

            // النتيجة النهائية كنسبة مئوية على 10
            double finalScore = (totalScore / maxScore) * 10;

            return Math.Round(finalScore, 2); // تقريبا لأقرب رقم عشري
        }
    }
}