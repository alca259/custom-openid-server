import { NgModule, APP_INITIALIZER } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { AppComponent } from './app.component';
import { routing } from './app.routes';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { HomeComponent } from './home/home.component';
import { UnauthorizedComponent } from './unauthorized/unauthorized.component';
import { AuthModule, LogLevel } from 'angular-auth-oidc-client';


@NgModule({
  imports: [
      BrowserModule,
      FormsModule,
      routing,
      HttpClientModule,
      AuthModule.forRoot({
        config: {
            triggerAuthorizationResultEvent: true,
            authority: 'https://localhost:5001',
            redirectUrl: 'http://localhost:6001/signin-oidc',
            postLogoutRedirectUri: 'http://localhost:6001/signout-oidc',
            clientId: 'angular-client',
            disablePkce: false,
            scope: 'openid profile email API offline_access',
            responseType: 'code',
            silentRenew: true,
            silentRenewUrl: 'http://localhost:6001/silent-renew.html',
            renewTimeBeforeTokenExpiresInSeconds: 10,
            useRefreshToken: true,
            logLevel: LogLevel.Debug,
            forbiddenRoute: '/unauthorized',
            unauthorizedRoute: '/unauthorized',
            postLoginRoute: '/home'
        },
      }),
  ],
  declarations: [
      AppComponent,
      HomeComponent,
      UnauthorizedComponent
  ],
  providers: [],
  bootstrap: [AppComponent],
})

export class AppModule {
  constructor() {
      console.log('APP STARTING');
  }
}
