import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { PanelModule } from 'primeng/panel';
import { ChartModule } from 'primeng/chart';

interface SummaryCard {
    title: string;
    value: string;
    subTitle?: string;
    trend?: 'up' | 'down' | 'flat';
}

interface TaskItem {
    title: string;
    module: string;
    dueDate: string;
    status: 'pending' | 'in_progress' | 'done';
    /** When set, row click navigates here (e.g. request-approval list). */
    routerLink?: string[];
    /** Optional query string for navigate (e.g. preset filters on target screen). */
    queryParams?: Record<string, string>;
}

interface PolicyItem {
    policyNo: string;
    customerName: string;
    createdAt: string;
    status: string;
}

interface ClaimItem {
    claimNo: string;
    customerName: string;
    createdAt: string;
    status: string;
}

@Component({
    selector: 'app-dashboard',
    standalone: true,
    imports: [CommonModule, PanelModule, ChartModule],
    templateUrl: './dashboard.component.html',
    styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
    constructor(private readonly router: Router) {}

    premiumBarData: any;
    premiumBarOptions: any;

    claimLineData: any;
    claimLineOptions: any;

    insuranceSummaryCards: SummaryCard[] = [
        {
            title: 'Doanh thu phí hôm nay',
            value: '1,25 tỷ',
            subTitle: 'So với hôm qua',
            trend: 'up'
        },
        {
            title: 'Doanh thu phí tháng này',
            value: '18,4 tỷ',
            subTitle: 'Trong tháng hiện tại',
            trend: 'up'
        },
        {
            title: 'Doanh thu phí năm nay',
            value: '210,8 tỷ',
            subTitle: 'Năm tài chính',
            trend: 'flat'
        },
        {
            title: 'Hợp đồng đang hiệu lực',
            value: '1.243',
            subTitle: 'Tổng số hợp đồng',
            trend: 'up'
        }
    ];

    claimSummaryCards: SummaryCard[] = [
        {
            title: 'Yêu cầu bồi thường mới hôm nay',
            value: '7',
            subTitle: 'Tiếp nhận trong ngày',
            trend: 'up'
        },
        {
            title: 'Hồ sơ chờ giám định',
            value: '23',
            subTitle: 'Cần phân công giám định',
            trend: 'flat'
        },
        {
            title: 'Hồ sơ đang xử lý',
            value: '54',
            subTitle: 'Đang thẩm định / duyệt',
            trend: 'up'
        },
        {
            title: 'Số tiền đã chi trả tháng này',
            value: '3,6 tỷ',
            subTitle: 'Đã thanh toán cho khách hàng',
            trend: 'up'
        }
    ];

    pendingTasks: TaskItem[] = [
        {
            title: 'Phê duyệt đơn bảo hiểm mới',
            module: 'Khai thác',
            dueDate: 'Hôm nay',
            status: 'pending',
            routerLink: ['/pages/policy/request-approval'],
            queryParams: { approvalType: 'request', approvalStatus: 'pending' }
        },
        {
            title: 'Giám định tổn thất hồ sơ bồi thường',
            module: 'Bồi thường',
            dueDate: 'Trong 4 giờ',
            status: 'in_progress'
        },
        {
            title: 'Nhắc phí đến hạn cho khách hàng',
            module: 'Khai thác',
            dueDate: 'Ngày mai',
            status: 'pending'
        }
    ];

    newPolicies: PolicyItem[] = [
        {
            policyNo: 'POL-2026-00045',
            customerName: 'Công ty ABC',
            createdAt: 'Hôm nay 10:15',
            status: 'Chờ phê duyệt'
        },
        {
            policyNo: 'POL-2026-00044',
            customerName: 'Nguyễn Văn A',
            createdAt: 'Hôm nay 09:40',
            status: 'Đang phát hành'
        },
        {
            policyNo: 'POL-2026-00043',
            customerName: 'Công ty XYZ',
            createdAt: 'Hôm qua 16:20',
            status: 'Đã phát hành'
        }
    ];

    newClaims: ClaimItem[] = [
        {
            claimNo: 'CLM-2026-00125',
            customerName: 'Công ty ABC',
            createdAt: 'Hôm nay 08:50',
            status: 'Chờ tiếp nhận'
        },
        {
            claimNo: 'CLM-2026-00124',
            customerName: 'Trần Thị B',
            createdAt: 'Hôm qua 15:10',
            status: 'Đang giám định'
        },
        {
            claimNo: 'CLM-2026-00123',
            customerName: 'Công ty XYZ',
            createdAt: 'Hôm qua 11:30',
            status: 'Đã phê duyệt'
        }
    ];

    ngOnInit(): void {
        this.initPremiumBarChart();
        this.initClaimLineChart();
    }

    private initPremiumBarChart(): void {
        const documentStyle = getComputedStyle(document.documentElement);
        const textColor = documentStyle.getPropertyValue('--text-color') || '#334155';
        const textColorSecondary = documentStyle.getPropertyValue('--text-color-secondary') || '#64748b';
        const surfaceBorder = documentStyle.getPropertyValue('--surface-border') || '#e2e8f0';

        this.premiumBarData = {
            labels: ['T1', 'T2', 'T3', 'T4', 'T5', 'T6'],
            datasets: [
                {
                    label: 'Doanh thu phí (tỷ)',
                    backgroundColor: '#f15a29',
                    borderRadius: 8,
                    data: [12.3, 14.8, 16.1, 17.5, 18.4, 19.2]
                }
            ]
        };

        this.premiumBarOptions = {
            maintainAspectRatio: false,
            aspectRatio: 2,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        usePointStyle: true,
                        color: textColor
                    }
                }
            },
            scales: {
                x: {
                    ticks: {
                        color: textColorSecondary
                    },
                    grid: {
                        display: false,
                        drawBorder: false
                    }
                },
                y: {
                    ticks: {
                        color: textColorSecondary,
                        callback: (value: any) => `${value}`
                    },
                    grid: {
                        color: surfaceBorder,
                        drawBorder: false
                    }
                }
            }
        };
    }

    private initClaimLineChart(): void {
        const documentStyle = getComputedStyle(document.documentElement);
        const textColor = documentStyle.getPropertyValue('--text-color') || '#334155';
        const textColorSecondary = documentStyle.getPropertyValue('--text-color-secondary') || '#64748b';
        const surfaceBorder = documentStyle.getPropertyValue('--surface-border') || '#e2e8f0';

        this.claimLineData = {
            labels: ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'],
            datasets: [
                {
                    label: 'Yêu cầu mới',
                    data: [4, 6, 5, 8, 7, 9, 6],
                    fill: false,
                    borderColor: '#f15a29',
                    tension: 0.3,
                    borderWidth: 1,
                    pointRadius: 3,
                    pointBackgroundColor: '#f15a29'
                },
                {
                    label: 'Đã giải quyết',
                    data: [2, 3, 4, 5, 6, 7, 5],
                    fill: false,
                    borderColor: '#155DFC',
                    tension: 0.3,
                    borderWidth: 1,
                    pointRadius: 3,
                    pointBackgroundColor: '#155DFC'
                }
            ]
        };

        this.claimLineOptions = {
            maintainAspectRatio: false,
            aspectRatio: 2,
            plugins: {
                legend: {
                    labels: {
                        usePointStyle: true,
                        color: textColor
                    },
                    position: 'bottom'
                }
            },
            scales: {
                x: {
                    ticks: {
                        color: textColorSecondary
                    },
                    grid: {
                        display: false,
                        drawBorder: false
                    }
                },
                y: {
                    ticks: {
                        color: textColorSecondary,
                        stepSize: 2
                    },
                    grid: {
                        color: surfaceBorder,
                        drawBorder: false
                    }
                }
            }
        };
    }

    onTaskRowClick(task: TaskItem): void {
        const link = task.routerLink;
        if (link?.length) {
            const qp = task.queryParams;
            this.router.navigate(link, qp && Object.keys(qp).length ? { queryParams: qp } : undefined);
        }
    }

    getTaskStatusClass(status: TaskItem['status']): string {
        switch (status) {
            case 'pending':
                return 'bg-[#f15a29]/10 text-[#f15a29]';
            case 'in_progress':
                return 'bg-[#155DFC]/10 text-[#155DFC]';
            case 'done':
                return 'bg-emerald-100 text-emerald-800';
            default:
                return 'bg-gray-100 text-gray-800';
        }
    }
}

