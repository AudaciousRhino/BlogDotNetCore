import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { ApplicationUserCreate } from '../models/account/application-user-create.model';
import { ApplicationUserLogin } from '../models/account/application-user-login.model';
import { ApplicationUser } from '../models/account/application-user.model';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  private currentUserSubject$: BehaviorSubject<ApplicationUser>

  constructor(
    private http: HttpClient
  ) {
    //this.currentUserSubject$ = new BehaviorSubject<ApplicationUser>(JSON.parse(localStorage.getItem('blogLab-currentUser')));
    /*
    Above/original line of code - error:
      "Argument of type 'string | null' is not assignable to parameter of type 'string'. Type 'null' is not assignable to type 'string'.ts(2345)"
    Solution: both lines below appear to work...  
    */
    this.currentUserSubject$ = new BehaviorSubject<ApplicationUser>(JSON.parse(localStorage.getItem('blogLab-currentUser')!));
    //or?...
    //this.currentUserSubject$ = new BehaviorSubject<ApplicationUser>(JSON.parse(localStorage.getItem('blogLab-currentUser') || '{}'));
  }

  login(model: ApplicationUserLogin) : Observable<ApplicationUser>  {
    return this.http.post(`${environment.webApi}/Account/login`, model).pipe(
      //map((user : ApplicationUser) => {
      /*
      Above/original line of code - error:
        "Argument of type 'OperatorFunction<ApplicationUser, ApplicationUser>' is not assignable to parameter of type 'OperatorFunction<Object, ApplicationUser>'.
        The 'Object' type is assignable to very few other types. Did you mean to use the 'any' type instead?
        Type 'Object' is missing the following properties from type 'ApplicationUser': applicationUserId, username, fullname, email, tokents(2345)"
      Solution: the line below appears to work (NOTE: The same for register() method below - i.e. use the 'any' type instead of 'ApplicationUser')...  
      */  
      map((user : any) => {  
        if (user) {
          localStorage.setItem('blogLab-currentUser', JSON.stringify(user));
          this.setCurrentUser(user);
        }

        return user;
      })
    )
  }

  register(model: ApplicationUserCreate) : Observable<ApplicationUser> {
    return this.http.post(`${environment.webApi}/Account/register`, model).pipe(
      map((user : any) => {

        if (user) {
          localStorage.setItem('blogLab-currentUser', JSON.stringify(user));
          this.setCurrentUser(user);
        }

        return user;
      })
    )
  }

  setCurrentUser(user: ApplicationUser) {
    this.currentUserSubject$.next(user);
  }

  public get currentUserValue(): ApplicationUser {
    return this.currentUserSubject$.value;
  }

  public givenUserIsLoggedIn(username: string) {
    return this.isLoggedIn() && this.currentUserValue.username === username;
  }

  public isLoggedIn() {
    const currentUser = this.currentUserValue;
    const isLoggedIn = !!currentUser && !!currentUser.token;
    return isLoggedIn;
  }

  logout() {
    localStorage.removeItem('blogLab-currentUser');
    //this.currentUserSubject$.next(null);
    /*
    Above/original line of code - error:
      Argument of type 'null' is not assignable to parameter of type 'ApplicationUser'.ts(2345)
    Solution:  
    */
    this.currentUserSubject$.next(null!);
  }
}
