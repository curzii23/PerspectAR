library(readxl)
library(ARTool)
library(dplyr)
library(ggplot2)

#https://rcompanion.org/handbook/F_16.html
cat("\014")

# Load the data from Excel file using a absolute path
df <- read_excel("Task_Time_Accuracy.xlsx", sheet = 1)

# Check and prepare your data
head(df)
str(df)

# Normality Check Function
check_normality <- function(data, variable) {
  
  # Shapiro-Wilk Test
  shapiro_test <- shapiro.test(data[[variable]])
  shapiro_p <- shapiro_test$p.value
  
  return(shapiro_p)
}

# Convert columns to appropriate types
df$Condition <- factor(df$Condition)
df$Visualization <- factor(df$Visualization)
df$PairID <- factor(df$PairID)
df$TaskAccuracy <- as.numeric(df$TaskAccuracy)


# Perform the normality check for TaskAccuracy
p_task_completion_time <- check_normality(df, 'TaskAccuracy')
print(paste("Shapiro-Wilk Test p-value for TaskAccuracy:", round(p_task_completion_time, 3)))

# Perform ART transformation for Task Accuracy
art_task_accuracy <- art(TaskAccuracy ~ Condition * Visualization + Error(PairID/(Condition*Visualization)), data = df)

anova_result = anova(art_task_accuracy)

print(anova_result)

anova_result$part.eta.sq = with(anova_result, `Sum Sq`/(`Sum Sq` + `Sum Sq.res`))

print(anova_result$part.eta.sq)


