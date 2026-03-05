import { Injectable } from '@angular/core';
import { ApiService } from './api';
import { Observable } from 'rxjs';

export interface NotificationDto {
    notificationId: number;
    message: string;
    isRead: boolean;
    createdAt: string;
}

@Injectable({
    providedIn: 'root'
})
export class NotificationService {
    constructor(private api: ApiService) { }

    getMyNotifications(unreadOnly = false): Observable<NotificationDto[]> {
        return this.api.get<NotificationDto[]>(`notifications?unreadOnly=${unreadOnly}`);
    }

    markAsRead(id: number): Observable<any> {
        return this.api.put(`notifications/${id}/read`, {});
    }
}
