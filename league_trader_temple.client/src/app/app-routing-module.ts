import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Home } from './home/home';
import { Search } from './search/search';
import { Card } from './card/card';
import { NotFound } from './not-found/not-found';

const routes: Routes = [
  { path: '', component: Home, pathMatch: 'full' },
  { path: 'search', component: Search },
  { path: 'card', component: Card },
  { path: '**', component: NotFound }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
