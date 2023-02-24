import { Component, OnDestroy, OnInit, Inject } from "@angular/core";
import { UntypedFormBuilder, UntypedFormGroup, Validators } from "@angular/forms";
import { UntypedFormBuilder, UntypedFormGroup, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { TranslateService } from "@ngx-translate/core";

import { APP_BASE_HREF } from "@angular/common";
import { AuthService } from "../auth/auth.service";
import { IAuthenticationSettings, ICustomizationSettings } from "../interfaces";
import { PlexTvService, StatusService, SettingsService } from "../services";

import { StorageService } from "../shared/storage/storage-service";
import { MatSnackBar } from "@angular/material/snack-bar";
import { CustomizationFacade } from "../state/customization";
import { SonarrFacade } from "app/state/sonarr";
import { RadarrFacade } from "app/state/radarr";

@Component({
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.scss"],
})
export class LoginComponent implements OnDestroy, OnInit {
  public form: UntypedFormGroup;
  public form: UntypedFormGroup;
  public customizationSettings: ICustomizationSettings;
  public authenticationSettings: IAuthenticationSettings;
  public plexEnabled: boolean;
  public landingFlag: boolean;
  public baseUrl: string;
  public loginWithOmbi: boolean;
  public pinTimer: any;
  public oauthLoading: boolean;

  public get appName(): string {
    if (this.customizationSettings.applicationName) {
      return this.customizationSettings.applicationName;
    } else {
      return "Ombi";
    }
  }

  public get appNameTranslate(): object {
    return { appName: this.appName };
  }
  private clientId: string;

  private errorBody: string;
  private errorValidation: string;
  private href: string;

  private oAuthWindow: Window | null;

  constructor(
    private authService: AuthService,
    private router: Router,
    private status: StatusService,
    private fb: UntypedFormBuilder,
    private fb: UntypedFormBuilder,
    private settingsService: SettingsService,
    private customziationFacade: CustomizationFacade,
    private route: ActivatedRoute,
    @Inject(APP_BASE_HREF) href: string,
    private translate: TranslateService,
    private plexTv: PlexTvService,
    private store: StorageService,
    private sonarrFacade: SonarrFacade,
    private radarrFacade: RadarrFacade,
    private readonly notify: MatSnackBar
  ) {
    this.href = href;
    this.route.params.subscribe((params: any) => {
      this.landingFlag = params.landing;
      if (!this.landingFlag) {
        this.settingsService.getLandingPage().subscribe((x) => {
          if (x.enabled && !this.landingFlag) {
            this.router.navigate(["landingpage"]);
          }
        });
      }
    });

    this.form = this.fb.group({
      username: ["", Validators.required],
      password: [""],
      rememberMe: [false],
    });

    this.status.getWizardStatus().subscribe((x) => {
      if (!x.result) {
        this.router.navigate(["Wizard"]);
      }
    });

    if (authService.loggedIn()) {
      this.loadStores();
      
      this.router.navigate(["/"]);
    }
  }

  public ngOnInit() {

    this.customziationFacade.settings$().subscribe(x => this.customizationSettings = x);

    this.settingsService
      .getAuthentication()
      .subscribe((x) => {
        this.authenticationSettings = x;
        this.headerAuth();
      });
    this.settingsService.getClientId().subscribe((x) => (this.clientId = x));

    const base = this.href;
    if (base.length > 1) {
      this.baseUrl = base;
    }
    this.translate
      .get("Login.Errors.IncorrectCredentials")
      .subscribe((x) => (this.errorBody = x));
    this.translate
      .get("Common.Errors.Validation")
      .subscribe((x) => (this.errorValidation = x));
  }

  public onSubmit(form: UntypedFormGroup) {
  public onSubmit(form: UntypedFormGroup) {
    if (form.invalid) {
      this.notify.open(this.errorValidation, "OK", {
        duration: 300000,
      });
      return;
    }
    const value = form.value;
    const user = {
      password: value.password,
      username: value.username,
      rememberMe: value.rememberMe,
      usePlexOAuth: false,
      plexTvPin: { id: 0, code: "" },
    };
    this.authService.requiresPassword(user).subscribe((x) => {
      if (x && this.authenticationSettings.allowNoPassword) {
        // Looks like this user requires a password
        this.authenticationSettings.allowNoPassword = false;
        return;
      }
      this.authService.login(user).subscribe(
        (x) => {
          this.store.save("id_token", x.access_token);

          if (this.authService.loggedIn()) {
            this.ngOnDestroy();
            this.loadStores();
            this.router.navigate(["/"]);
          } else {
            this.notify.open(this.errorBody, "OK", {
              duration: 3000,
            });
          }
        },
        (err) => {
          this.notify.open(this.errorBody, "OK", {
            duration: 3000000,
          });
        }
      );
    });
  }

  public oauth() {
    if (this.oAuthWindow) {
      this.oAuthWindow.close();
    }

    this.oAuthWindow = window.open(
      window.location.toString(),
      "_blank",
      `toolbar=0,
        location=0,
        status=0,
        menubar=0,
        scrollbars=1,
        resizable=1,
        width=500,
        height=500`
    );
    this.plexTv.GetPin(this.clientId, this.appName).subscribe((pin: any) => {
      this.authService
        .login({
          usePlexOAuth: true,
          password: "",
          rememberMe: true,
          username: "",
          plexTvPin: pin,
        })
        .subscribe((x) => {
          this.oAuthWindow!.location.replace(x.url);

          if (this.pinTimer) {
            clearInterval(this.pinTimer);
          }

          this.pinTimer = setInterval(() => {
              this.oauthLoading = true;
              this.getPinResult(x.pinId);
          }, 1000);
        });
    });
  }

  public getPinResult(pinId: number) {
    if (this.oAuthWindow.closed) {
        if (this.pinTimer) {
          clearInterval(this.pinTimer);
        }
    }
    this.authService.oAuth(pinId).subscribe(
      (x) => {
        if (x.access_token) {
          clearInterval(this.pinTimer);
          this.store.save("id_token", x.access_token);

          if (this.authService.loggedIn()) {
            this.ngOnDestroy();

            if (this.oAuthWindow) {
              this.oAuthWindow.close();
            }
            this.oauthLoading = false;
            this.loadStores();
            this.router.navigate([""]);
            return;
          }
        }
        // if (notifyUser) {
        //   this.notify.open("Could not log you in!", "OK", {
        //     duration: 3000,
        //   });
        // }
        this.oauthLoading = false;
      },
      (err) => {
        console.log(err);
        this.notify.open("You are not authenticated with Ombi", "OK", {
          duration: 3000,
        });

        this.router.navigate(["login"]);
      }
    );
  }

  public headerAuth() {
    if (this.authenticationSettings.enableHeaderAuth) {
      this.authService.headerAuth().subscribe({
        next: (x) => {
          this.store.save("id_token", x.access_token);

          if (this.authService.loggedIn()) {
            this.ngOnDestroy();
            this.loadStores();
            this.router.navigate(["/"]);
          } else {
            this.notify.open(this.errorBody, "OK", {
              duration: 3000,
            });
          }
        },
        error: (e) => {
          this.notify.open(this.errorBody, "OK", {
            duration: 3000000,
          });
          console.error(e);
        }
      }
      );
    }
  }

  public ngOnDestroy() {
    clearInterval(this.pinTimer);
  }

  private loadStores() {
    this.sonarrFacade.load().subscribe();
    this.radarrFacade.load().subscribe();
  }
}
