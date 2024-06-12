﻿using System.Data;
using TP.Entitiy;

namespace TP.control
{
    internal class SalesInfoController
    {
        private ReceiptList receiptList;

        public SalesInfoController()
        {
            receiptList = new ReceiptList();
        }

        public DataTable getMargin(int year)
        {
            string sqltxt = $@"SELECT 
                        EXTRACT(MONTH FROM r.거래시간) AS 월,
                        SUM(CASE WHEN r.거래형태 = '환불' THEN -1 * rs.수량 * rs.상품가격 ELSE rs.수량 * rs.상품가격 END) AS 총판매액,
                        SUM(CASE WHEN r.거래형태 = '환불' THEN -1 * rs.수량 * p.단가 ELSE rs.수량 * p.단가 END) AS 총원가
                        FROM 
                        영수증 r
                        JOIN 
                        영수증상세 rs ON r.영수증번호 = rs.영수증번호
                        JOIN 
                        제품 p ON rs.제품번호 = p.제품번호
                        WHERE 
                        EXTRACT(YEAR FROM r.거래시간) = {year}
                        GROUP BY 
                        EXTRACT(MONTH FROM r.거래시간);";

            return receiptList.GetReceipt(sqltxt);
        }
        public DataTable salesCalc(int type, string date)
        {
            string sqltxt;
            if (type == 1)
            { //일일판매실적
                sqltxt = $@"
                        SELECT 시간, 결제금액, 반품금액, 객수, 판매금액
                        FROM 거래내역
                        WHERE 거래일 = TO_DATE('{date}', 'YYYY-MM-DD')";
                //MessageBox.Show(sqltxt);
                return receiptList.GetReceipt(sqltxt);
            }
            else if (type == 2)
            {
                //월별판매실적    
                sqltxt = $@"select 일자,결제금액,반품금액,객수,판매금액 
                            from 월별거래내역 
                            where TO_CHAR(TO_DATE(거래일자, 'YYYY-MM-DD'), 'YYYY-MM') ='{date}'";
                return receiptList.GetReceipt(sqltxt);
            }
            else if (type == 3)
            {
                //대분류별판매실적 
                sqltxt = $@"select 대분류,수량,결제금액
                            from 대분류별실적
                            where TO_CHAR(TO_DATE(거래일자, 'YYYY-MM-DD'), 'YYYY-MM') = '{date}'";
                return receiptList.GetReceipt(sqltxt);
            }
            else
            {
                return null;
            }
        }
    }
}