package gemini.workshop;

import dev.langchain4j.model.chat.ChatLanguageModel;
import dev.langchain4j.model.vertexai.VertexAiGeminiChatModel;
import dev.langchain4j.data.message.AiMessage;
import dev.langchain4j.model.input.Prompt;
import dev.langchain4j.model.input.PromptTemplate;
import dev.langchain4j.model.output.Response;

import java.util.HashMap;
import java.util.Map;

public class TemplatePrompt {
    public static void main(String[] args) {
        ChatLanguageModel model = VertexAiGeminiChatModel.builder()
            .project(System.getenv("PROJECT_ID"))
            .location(System.getenv("LOCATION"))
            .modelName("gemini-2.0-flash")
            .maxOutputTokens(500)
            .temperature(1.0f)
            .topK(40)
            .topP(0.95f)
            .maxRetries(3)
            .build();

        PromptTemplate promptTemplate = PromptTemplate.from("""
            You're a friendly chef with a lot of cooking experience.
            Create a recipe for a {{dish}} with the following ingredients: \
            {{ingredients}}, and give it a name.
            """
        );

        Map<String, Object> variables = new HashMap<>();
        variables.put("dish", "dessert");
        variables.put("ingredients", "strawberries, chocolate, and whipped cream");

        Prompt prompt = promptTemplate.apply(variables);

        Response<AiMessage> response = model.generate(prompt.toUserMessage());

        System.out.println(response.content().text());
    }
}
