import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { AbpOAuthService } from '@abp/ng.oauth';
import { environment } from '@environments/environment';
import { Subject } from 'rxjs';
import { AuthService, ConfigStateService } from '@abp/ng.core';

@Injectable({
    providedIn: 'root'
})
export class NotificationSignalrService {
    private hubConnection: signalR.HubConnection | undefined;
    private oauthService = inject(AbpOAuthService);
    private authService = inject(AuthService);
    private configState = inject(ConfigStateService);

    private notificationReceived = new Subject<any>();
    notificationReceived$ = this.notificationReceived.asObservable();

    private currentUserId: string | null = null;

    constructor() {
        this.configState.getOne$('currentUser').subscribe((user: any) => {
            if (user && user.id) {
                this.currentUserId = user.id;
                this.startConnection();
            } else {
                this.currentUserId = null;
                this.stopConnection();
            }
        });
    }

    public startConnection() {
        if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            return;
        }

        const hubUrl = `${environment.apis.default.url}/signalr-hubs/notification`;

        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, {
                accessTokenFactory: () => this.oauthService.getAccessToken()
            })
            .withAutomaticReconnect()
            .build();

        this.hubConnection
            .start()
            .then(() => {
                console.log('SignalR connection started to ' + hubUrl);
                if (this.currentUserId) {
                    // Join the user group after connecting
                    this.hubConnection?.invoke('JoinUserGroup', this.currentUserId)
                        .then(() => console.log(`Joined SignalR group for user: ${this.currentUserId}`))
                        .catch((err: unknown) => console.error('Error while joining user group: ', err));
                }
            })
            .catch((err: unknown) => console.error('Error while starting SignalR connection: ', err));

        this.hubConnection.on('ReceiveNotification', (notification: unknown) => {
            console.log('Notification received via SignalR:', notification);
            this.notificationReceived.next(notification);
        });
    }

    public stopConnection() {
        if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
            if (this.currentUserId) {
                this.hubConnection.invoke('LeaveUserGroup', this.currentUserId)
                    .catch((err: unknown) => console.error('Error while leaving user group: ', err))
                    .finally(() => {
                        this.hubConnection?.stop().then(() => console.log('SignalR connection stopped'));
                    });
            } else {
                this.hubConnection.stop().then(() => console.log('SignalR connection stopped'));
            }
        }
    }
}
