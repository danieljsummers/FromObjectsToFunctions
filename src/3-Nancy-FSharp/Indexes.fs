module Tres.Indexes

open Raven.Client.Documents.Indexes
open System.Collections.Generic

type Categories_ByWebLogIdAndSlug () as this =
  inherit AbstractJavaScriptIndexCreationTask ()
  do
    this.Maps <-
      HashSet<string> [
        "map('Categories', category => {
          return {
            WebLogId : category.WebLogId,
            Slug     : category.Slug
          }
        })"
        ]
        
type Comments_ByPostId () as this =
  inherit AbstractJavaScriptIndexCreationTask ()
  do
    this.Maps <-
      HashSet<string> [
        "map('Comments', comment => {
          return {
            PostId : comment.PostId
          }
        })"
        ]

type Pages_ByWebLogIdAndPermalink () as this =
  inherit AbstractJavaScriptIndexCreationTask ()
  do
    this.Maps <-
      HashSet<string> [
        "map('Pages', page => {
          return {
            WebLogId  : page.WebLogId,
            Permalink : page.Permalink
          }
        })"
        ]
    
type Posts_ByWebLogIdAndPermalink () as this =
  inherit AbstractJavaScriptIndexCreationTask ()
  do
    this.Maps <-
      HashSet<string> [
        "map('Posts', post => {
          return {
            WebLogId  : post.WebLogId,
            Permalink : post.Permalink
            }
          })"
        ]
    
type Posts_ByWebLogIdAndCategoryId () as this =
  inherit AbstractJavaScriptIndexCreationTask ()
  do
    this.Maps <-
      HashSet<string> [
        "docs.Posts.SelectMany(post => post.CategoryIds, (post, category) => new {
          WebLogId   = post.WebLogId,
          CategoryId = category
        })"
        ]

type Posts_ByWebLogIdAndTag () as this =
  inherit AbstractJavaScriptIndexCreationTask ()
  do
    this.Maps <-
      HashSet<string> [
        "docs.Posts.SelectMany(post => post.Tags, (post, tag) => new {
          Id  = Id(post),
          Tag = tag
        })"
        ]

type Users_ByEmailAddressAndPasswordHash () as this =
  inherit AbstractJavaScriptIndexCreationTask ()
  do
    this.Maps <-
      HashSet<string> [
        "map('Users', user => {
          return {
            EmailAddress : user.EmailAddress,
            PasswordHash : user.PasswordHash
          }
        })"
        ]
